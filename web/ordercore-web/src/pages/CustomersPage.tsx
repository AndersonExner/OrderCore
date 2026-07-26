import { useCallback, useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";

import Modal from "../components/Modal";
import { createCustomer, getCustomers, searchCustomer } from "../api/customers";
import type { CustomerResponse } from "../api/customers";

type Feedback = {
  type: "success" | "error";
  text: string;
};

function getShortId(id: string) {
  return id.slice(0, 8);
}

export default function CustomersPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [customerSearch, setCustomerSearch] = useState("");
  const [customer, setCustomer] = useState<CustomerResponse | null>(null);
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isSearchModalOpen, setIsSearchModalOpen] = useState(false);
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  const emailDomains = useMemo(() => {
    return new Set(
      customers
        .map((currentCustomer) => currentCustomer.email.split("@")[1])
        .filter((domain): domain is string => Boolean(domain))
    ).size;
  }, [customers]);

  const loadCustomers = useCallback(async () => {
    try {
      setLoading(true);
      const result = await getCustomers();
      setCustomers(result);
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to load customers.",
      });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadCustomers();
  }, [loadCustomers]);

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);

    try {
      const createdCustomer = await createCustomer({ name, email });

      setName("");
      setEmail("");
      setCustomer(createdCustomer);
      setIsCreateModalOpen(false);
      setFeedback({ type: "success", text: "Customer created successfully." });
      await loadCustomers();
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to create customer.",
      });
    }
  }

  async function handleSearch(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);

    try {
      const result = await searchCustomer(customerSearch);
      setCustomer(result);
    } catch (error) {
      setCustomer(null);
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to load customer.",
      });
    }
  }

  return (
    <div className="page-shell">
      <header className="page-header">
        <div>
          <p className="page-kicker">Registry</p>
          <h1 className="page-title">Customers</h1>
        </div>

        <div className="button-row">
          <button
            type="button"
            onClick={() => {
              setFeedback(null);
              void loadCustomers();
            }}
            className="button secondary"
          >
            Refresh data
          </button>
          <button
            type="button"
            onClick={() => setIsCreateModalOpen(true)}
            className="button primary"
          >
            New customer
          </button>
        </div>
      </header>

      <section className="grid stats">
        <div className="panel stat-card">
          <span className="stat-label">Total customers</span>
          <strong className="stat-value">{customers.length}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Email domains</span>
          <strong className="stat-value">{emailDomains}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Selected</span>
          <strong className="stat-value">{customer ? `#${getShortId(customer.id)}` : "-"}</strong>
        </div>
      </section>

      {feedback && (
        <div className={`feedback ${feedback.type}`} role="status">
          {feedback.text}
        </div>
      )}

      <section className="panel">
        <div className="panel-header">
          <div>
            <h2 className="panel-title">Customer list</h2>
            {loading && <span className="muted">Loading...</span>}
          </div>
          <button
            type="button"
            className="icon-button"
            onClick={() => setIsSearchModalOpen(true)}
            aria-label="Search customer"
            title="Search customer"
          >
            <span className="search-icon" aria-hidden="true" />
          </button>
        </div>

        {customers.length === 0 ? (
          <div className="empty-state">No customers found.</div>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Customer</th>
                  <th>Email</th>
                  <th>Id</th>
                </tr>
              </thead>
              <tbody>
                {customers.map((currentCustomer) => (
                  <tr key={currentCustomer.id}>
                    <td>
                      <strong className="entity-name">{currentCustomer.name}</strong>
                    </td>
                    <td>{currentCustomer.email}</td>
                    <td>
                      <span className="entity-id">{currentCustomer.id}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <Modal
        title="New customer"
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      >
        <form onSubmit={handleCreate} className="form-grid">
          <label className="field-label">
            Name
            <input
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="field"
              autoComplete="name"
            />
          </label>

          <label className="field-label">
            Email
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="field"
              autoComplete="email"
            />
          </label>

          <div className="button-row">
            <button
              type="button"
              onClick={() => setIsCreateModalOpen(false)}
              className="button secondary"
            >
              Cancel
            </button>
            <button type="submit" className="button primary">
              Create customer
            </button>
          </div>
        </form>
      </Modal>

      <Modal
        title="Find customer"
        isOpen={isSearchModalOpen}
        onClose={() => setIsSearchModalOpen(false)}
      >
        <form onSubmit={handleSearch} className="form-grid">
          <label className="field-label">
            Search term
            <input
              value={customerSearch}
              onChange={(event) => setCustomerSearch(event.target.value)}
              className="field"
              autoFocus
            />
          </label>

          <div className="button-row">
            <button
              type="button"
              onClick={() => setIsSearchModalOpen(false)}
              className="button secondary"
            >
              Cancel
            </button>
            <button type="submit" className="button primary">
              Search
            </button>
          </div>

          {customer && (
            <div className="result-box">
              <strong className="entity-name">{customer.name}</strong>
              <span className="muted">{customer.email}</span>
              <div className="entity-id">{customer.id}</div>
            </div>
          )}
        </form>
      </Modal>
    </div>
  );
}
