import { useEffect, useState } from "react";
import type { FormEvent } from "react";

import {
  createCustomer,
  getCustomers,
} from "../api/customers";

import type { CustomerResponse } from "../api/customers";

export default function CustomersPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");

  async function loadCustomers() {
    try {
      setLoading(true);
      setMessage("");
      const result = await getCustomers();
      setCustomers(result);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to load customers.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadCustomers();
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    if (!name.trim()) {
      setMessage("Customer name is required.");
      return;
    }

    if (!email.trim()) {
      setMessage("Customer email is required.");
      return;
    }

    try {
      setSubmitting(true);

      await createCustomer({
        name: name.trim(),
        email: email.trim(),
      });

      setName("");
      setEmail("");
      setMessage("Customer created successfully.");
      await loadCustomers();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to create customer.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <h2>Customers</h2>

      <form
        onSubmit={handleSubmit}
        style={{
          background: "white",
          padding: "20px",
          borderRadius: "12px",
          boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
          marginBottom: "24px",
          display: "grid",
          gap: "12px",
          maxWidth: "500px",
        }}
      >
        <input
          placeholder="Customer name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <input
          placeholder="Customer email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <button
          type="submit"
          disabled={submitting}
          style={{
            padding: "10px 14px",
            borderRadius: "8px",
            border: "none",
            background: "#111827",
            color: "white",
            cursor: submitting ? "not-allowed" : "pointer",
            opacity: submitting ? 0.7 : 1,
          }}
        >
          {submitting ? "Creating..." : "Create customer"}
        </button>
      </form>

      {message && <p>{message}</p>}

      <div>
        <h3>Customer list</h3>
        {loading ? (
          <p>Loading...</p>
        ) : customers.length === 0 ? (
          <p>No customers found.</p>
        ) : (
          <table
            style={{
              width: "100%",
              borderCollapse: "collapse",
              background: "white",
              borderRadius: "12px",
              overflow: "hidden",
              boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
            }}
          >
            <thead>
              <tr style={{ background: "#e5e7eb" }}>
                <th style={{ textAlign: "left", padding: "12px" }}>Name</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Email</th>
              </tr>
            </thead>
            <tbody>
              {customers.map((customer) => (
                <tr key={customer.id}>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {customer.name}
                  </td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {customer.email}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}