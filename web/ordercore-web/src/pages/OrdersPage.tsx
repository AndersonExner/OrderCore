import { useEffect, useState } from "react";
import type { FormEvent } from "react";

import { createOrder, getOrders } from "../api/orders";
import type { OrderSummaryResponse } from "../api/orders";

export default function OrdersPage() {
  const [customerId, setCustomerId] = useState("");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [orders, setOrders] = useState<OrderSummaryResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  async function loadOrders() {
    try {
      setLoading(true);
      const result = await getOrders();
      setOrders(result);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to load orders.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadOrders();
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    try {
      await createOrder({
        customerId,
        items: [
          {
            productId,
            quantity: Number(quantity),
          },
        ],
      });

      setProductId("");
      setQuantity("1");
      setMessage("Order created successfully.");
      await loadOrders();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to create order.");
    }
  }

  return (
    <div>
      <h2>Orders</h2>

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
          maxWidth: "620px",
        }}
      >
        <input
          placeholder="Customer id"
          value={customerId}
          onChange={(e) => setCustomerId(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <input
          placeholder="Product id"
          value={productId}
          onChange={(e) => setProductId(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <input
          placeholder="Quantity"
          type="number"
          min="1"
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
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
          Create order
        </button>
      </form>

      {message && <p>{message}</p>}

      <div>
        <h3>Order list</h3>
        {loading ? (
          <p>Loading...</p>
        ) : orders.length === 0 ? (
          <p>No orders found.</p>
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
                <th style={{ textAlign: "left", padding: "12px" }}>Created</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Customer</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Status</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Total</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {new Date(order.createdAtUtc).toLocaleString()}
                  </td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {order.customerId}
                  </td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {order.status}
                  </td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                    {order.totalAmount}
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
