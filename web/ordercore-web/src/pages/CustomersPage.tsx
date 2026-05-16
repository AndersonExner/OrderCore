import { useState } from "react";
import type { FormEvent } from "react";

import { createCustomer, getCustomerById } from "../api/customers";
import type { CustomerResponse } from "../api/customers";

export default function CustomersPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [customerId, setCustomerId] = useState("");
  const [customer, setCustomer] = useState<CustomerResponse | null>(null);
  const [message, setMessage] = useState("");

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    try {
      const createdCustomer = await createCustomer({ name, email });

      setName("");
      setEmail("");
      setCustomer(createdCustomer);
      setMessage("Customer created successfully.");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to create customer.");
    }
  }

  async function handleSearch(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    try {
      const result = await getCustomerById(customerId);
      setCustomer(result);
    } catch (error) {
      setCustomer(null);
      setMessage(error instanceof Error ? error.message : "Failed to load customer.");
    }
  }

  return (
    <div>
      <h2>Customers</h2>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
          gap: "20px",
          marginBottom: "24px",
        }}
      >
        <form
          onSubmit={handleCreate}
          style={{
            background: "white",
            padding: "20px",
            borderRadius: "12px",
            boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
            display: "grid",
            gap: "12px",
          }}
        >
          <h3 style={{ margin: 0 }}>Create customer</h3>

          <input
            placeholder="Customer name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
          />

          <input
            placeholder="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
          />

          <button
            type="submit"
            style={{
              padding: "10px 14px",
              borderRadius: "8px",
              border: "none",
              background: "#111827",
              color: "white",
              cursor: "pointer",
            }}
          >
            Create customer
          </button>
        </form>

        <form
          onSubmit={handleSearch}
          style={{
            background: "white",
            padding: "20px",
            borderRadius: "12px",
            boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
            display: "grid",
            gap: "12px",
          }}
        >
          <h3 style={{ margin: 0 }}>Find customer</h3>

          <input
            placeholder="Customer id"
            value={customerId}
            onChange={(e) => setCustomerId(e.target.value)}
            style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
          />

          <button
            type="submit"
            style={{
              padding: "10px 14px",
              borderRadius: "8px",
              border: "none",
              background: "#111827",
              color: "white",
              cursor: "pointer",
            }}
          >
            Find customer
          </button>
        </form>
      </div>

      {message && <p>{message}</p>}

      {customer && (
        <section
          style={{
            background: "white",
            padding: "20px",
            borderRadius: "12px",
            boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
          }}
        >
          <h3 style={{ marginTop: 0 }}>{customer.name}</h3>
          <p style={{ margin: "8px 0" }}>{customer.email}</p>
          <p style={{ margin: 0, color: "#4b5563" }}>{customer.id}</p>
        </section>
      )}
    </div>
  );
}
