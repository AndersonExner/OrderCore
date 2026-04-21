import { useEffect, useState } from "react";
import type { FormEvent } from "react";

import { createOrder, getOrders, getOrderById } from "../api/orders";
import { getCustomers } from "../api/customers";
import { getProducts } from "../api/products";

import type { OrderResponse } from "../api/orders";
import type { OrderListResponse } from "../api/orders";
import type { CustomerResponse } from "../api/customers";
import type { ProductResponse } from "../api/products";

type OrderFormItem = {
  productId: string;
  quantity: string;
};

export default function OrdersPage() {
  const [customerId, setCustomerId] = useState("");
  const [items, setItems] = useState<OrderFormItem[]>([
    { productId: "", quantity: "1" },
  ]);

  const [orders, setOrders] = useState<OrderListResponse[]>([]);
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [products, setProducts] = useState<ProductResponse[]>([]);

  const [selectedOrder, setSelectedOrder] = useState<OrderResponse | null>(null);
  const [loadingDetails, setLoadingDetails] = useState(false);  

  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");

  async function loadPageData() {
    try {
      setLoading(true);
      setMessage("");

      const [ordersResult, customersResult, productsResult] = await Promise.all(
        [getOrders(), getCustomers(), getProducts()],
      );

      setOrders(ordersResult);
      setCustomers(customersResult);
      setProducts(productsResult);
    } catch (error) {
      setMessage(
        error instanceof Error ? error.message : "Failed to load page data.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPageData();
  }, []);

  function handleItemChange(
    index: number,
    field: keyof OrderFormItem,
    value: string,
  ) {
    setItems((currentItems) =>
      currentItems.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item,
      ),
    );
  }

  function addItem() {
    setItems((currentItems) => [
      ...currentItems,
      { productId: "", quantity: "1" },
    ]);
  }

  function removeItem(index: number) {
    setItems((currentItems) =>
      currentItems.filter((_, itemIndex) => itemIndex !== index),
    );
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    if (!customerId) {
      setMessage("Customer is required.");
      return;
    }

    if (items.length === 0) {
      setMessage("At least one item is required.");
      return;
    }

    const parsedItems = items.map((item) => ({
      productId: item.productId,
      quantity: Number(item.quantity),
    }));

    const hasInvalidItem = parsedItems.some(
      (item) =>
        !item.productId || Number.isNaN(item.quantity) || item.quantity <= 0,
    );

    if (hasInvalidItem) {
      setMessage(
        "All items must have a product and quantity greater than zero.",
      );
      return;
    }

    try {
      setSubmitting(true);

      await createOrder({
        customerId,
        items: parsedItems,
      });

      setCustomerId("");
      setItems([{ productId: "", quantity: "1" }]);
      setMessage("Order created successfully.");

      await loadPageData();
    } catch (error) {
      setMessage(
        error instanceof Error ? error.message : "Failed to create order.",
      );
    } finally {
      setSubmitting(false);
      setSelectedOrder(null);
    }
  }

  async function handleViewDetails(orderId: string) {
    try {
      setMessage("");
      setLoadingDetails(true);
      const order = await getOrderById(orderId);
      setSelectedOrder(order);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to load order details.");
    } finally {
      setLoadingDetails(false);
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
          gap: "16px",
          maxWidth: "700px",
        }}
      >
        <div style={{ display: "grid", gap: "8px" }}>
          <label>Customer</label>
          <select
            value={customerId}
            onChange={(e) => setCustomerId(e.target.value)}
            style={{
              padding: "10px",
              borderRadius: "8px",
              border: "1px solid #d1d5db",
            }}
          >
            <option value="">Select a customer</option>
            {customers.map((customer) => (
              <option key={customer.id} value={customer.id}>
                {customer.name} - {customer.email}
              </option>
            ))}
          </select>
        </div>

        <div style={{ display: "grid", gap: "12px" }}>
          <h3 style={{ margin: 0 }}>Items</h3>

          {items.map((item, index) => (
            <div
              key={index}
              style={{
                display: "grid",
                gridTemplateColumns: "1fr 140px 120px",
                gap: "12px",
                alignItems: "end",
              }}
            >
              <div style={{ display: "grid", gap: "8px" }}>
                <label>Product</label>
                <select
                  value={item.productId}
                  onChange={(e) =>
                    handleItemChange(index, "productId", e.target.value)
                  }
                  style={{
                    padding: "10px",
                    borderRadius: "8px",
                    border: "1px solid #d1d5db",
                  }}
                >
                  <option value="">Select a product</option>
                  {products.map((product) => (
                    <option key={product.id} value={product.id}>
                      {product.name} - {product.price.toFixed(2)}
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: "grid", gap: "8px" }}>
                <label>Quantity</label>
                <input
                  type="number"
                  min="1"
                  value={item.quantity}
                  onChange={(e) =>
                    handleItemChange(index, "quantity", e.target.value)
                  }
                  style={{
                    padding: "10px",
                    borderRadius: "8px",
                    border: "1px solid #d1d5db",
                  }}
                />
              </div>

              <button
                type="button"
                onClick={() => removeItem(index)}
                disabled={items.length === 1}
                style={{
                  padding: "10px 14px",
                  borderRadius: "8px",
                  border: "1px solid #d1d5db",
                  background: "white",
                  cursor: items.length === 1 ? "not-allowed" : "pointer",
                  opacity: items.length === 1 ? 0.6 : 1,
                }}
              >
                Remove
              </button>
            </div>
          ))}

          <button
            type="button"
            onClick={addItem}
            style={{
              padding: "10px 14px",
              borderRadius: "8px",
              border: "1px solid #d1d5db",
              background: "white",
              cursor: "pointer",
              width: "fit-content",
            }}
          >
            Add item
          </button>
        </div>

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
          {submitting ? "Creating..." : "Create order"}
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
                <th style={{ textAlign: "left", padding: "12px" }}>Order</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Customer</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Status</th>
                <th style={{ textAlign: "left", padding: "12px" }}>
                  Created at
                </th>
                <th style={{ textAlign: "left", padding: "12px" }}>Total</th>
                <th style={{ padding: "12px" }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td
                    style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}
                  >
                    {order.id}
                  </td>
                  <td
                    style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}
                  >
                    {order.customerId}
                  </td>
                  <td
                    style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}
                  >
                    {order.status}
                  </td>
                  <td
                    style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}
                  >
                    {new Date(order.createdAtUtc).toLocaleString()}
                  </td>
                  <td
                    style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}
                  >
                    {order.totalAmount.toFixed(2)}
                  </td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                  <button
                    type="button"
                    onClick={() => handleViewDetails(order.id)}
                    style={{
                      padding: "6px 10px",
                      borderRadius: "6px",
                      border: "1px solid #d1d5db",
                      background: "white",
                      cursor: "pointer",
                    }}
                  >
                    Details
                  </button>
                </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {loadingDetails && <p>Loading order details...</p>}

        {selectedOrder && (
          <div
            style={{
              marginTop: "24px",
              background: "white",
              padding: "16px",
              borderRadius: "12px",
              boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
            }}
          >
            <h3>Order details</h3>

            <p><strong>Order:</strong> {selectedOrder.id}</p>
            <p><strong>Customer:</strong> {selectedOrder.customerId}</p>
            <p><strong>Status:</strong> {selectedOrder.status}</p>
            <p><strong>Created at:</strong> {new Date(selectedOrder.createdAtUtc).toLocaleString()}</p>
            <p><strong>Total:</strong> {selectedOrder.totalAmount.toFixed(2)}</p>

            <table
              style={{
                width: "100%",
                borderCollapse: "collapse",
                marginTop: "12px",
              }}
            >
              <thead>
                <tr style={{ background: "#e5e7eb" }}>
                  <th style={{ padding: "12px" }}>Product</th>
                  <th style={{ padding: "12px" }}>Unit price</th>
                  <th style={{ padding: "12px" }}>Quantity</th>
                  <th style={{ padding: "12px" }}>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedOrder.items.map((item, index) => (
                  <tr key={index}>
                    <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                      {item.productName}
                    </td>
                    <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                      {item.unitPrice.toFixed(2)}
                    </td>
                    <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                      {item.quantity}
                    </td>
                    <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>
                      {item.total.toFixed(2)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <button
              onClick={() => setSelectedOrder(null)}
              style={{
                marginTop: "12px",
                padding: "8px 12px",
                borderRadius: "8px",
                border: "none",
                background: "#ef4444",
                color: "white",
                cursor: "pointer",
              }}
            >
              Close
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
