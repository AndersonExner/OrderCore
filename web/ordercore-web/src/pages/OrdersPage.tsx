import { useEffect, useState } from "react";
import type { FormEvent } from "react";

import { cancelOrder, createOrder, getOrders, payOrder } from "../api/orders";
import type { OrderSummaryResponse } from "../api/orders";

export default function OrdersPage() {
  const [customerId, setCustomerId] = useState("");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [orders, setOrders] = useState<OrderSummaryResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [processingOrderId, setProcessingOrderId] = useState("");
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

  async function handlePay(orderId: string) {
    setMessage("");

    try {
      setProcessingOrderId(orderId);
      await payOrder(orderId);
      setMessage("Order paid successfully.");
      await loadOrders();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to pay order.");
    } finally {
      setProcessingOrderId("");
    }
  }

  async function handleCancel(orderId: string) {
    setMessage("");

    try {
      setProcessingOrderId(orderId);
      await cancelOrder(orderId);
      setMessage("Order cancelled successfully.");
      await loadOrders();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to cancel order.");
    } finally {
      setProcessingOrderId("");
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
                <th style={{ textAlign: "left", padding: "12px" }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => {
                const canProcess = order.status === "Pending";
                const isProcessing = processingOrderId === order.id;

                return (
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
                    <td
                      style={{
                        padding: "12px",
                        borderTop: "1px solid #e5e7eb",
                        display: "flex",
                        gap: "8px",
                      }}
                    >
                      <button
                        type="button"
                        disabled={!canProcess || isProcessing}
                        onClick={() => void handlePay(order.id)}
                        style={{
                          padding: "8px 10px",
                          borderRadius: "8px",
                          border: "none",
                          background: canProcess ? "#047857" : "#9ca3af",
                          color: "white",
                          cursor: canProcess ? "pointer" : "not-allowed",
                        }}
                      >
                        Pay
                      </button>
                      <button
                        type="button"
                        disabled={!canProcess || isProcessing}
                        onClick={() => void handleCancel(order.id)}
                        style={{
                          padding: "8px 10px",
                          borderRadius: "8px",
                          border: "none",
                          background: canProcess ? "#b91c1c" : "#9ca3af",
                          color: "white",
                          cursor: canProcess ? "pointer" : "not-allowed",
                        }}
                      >
                        Cancel
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
