import { useCallback, useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";

import { getCustomers } from "../api/customers";
import type { CustomerResponse } from "../api/customers";
import { cancelOrder, createOrder, getOrders, payOrder } from "../api/orders";
import type { OrderSummaryResponse } from "../api/orders";
import { getProducts } from "../api/products";
import type { ProductResponse } from "../api/products";
import Modal from "../components/Modal";

type OrderDraftItem = {
  id: string;
  productId: string;
  quantity: string;
};

type Feedback = {
  type: "success" | "error";
  text: string;
};

function createDraftItem(): OrderDraftItem {
  return {
    id: globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(36).slice(2)}`,
    productId: "",
    quantity: "1",
  };
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function getShortId(id: string) {
  return id.slice(0, 8);
}

function getStatusClass(status: string) {
  if (status === "Paid") {
    return "paid";
  }

  if (status === "Cancelled") {
    return "cancelled";
  }

  return "pending";
}

export default function OrdersPage() {
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [orders, setOrders] = useState<OrderSummaryResponse[]>([]);
  const [customerId, setCustomerId] = useState("");
  const [items, setItems] = useState<OrderDraftItem[]>([createDraftItem()]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [processingOrderId, setProcessingOrderId] = useState("");
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  const productsById = useMemo(
    () => new Map(products.map((product) => [product.id, product])),
    [products]
  );

  const customersById = useMemo(
    () => new Map(customers.map((customer) => [customer.id, customer])),
    [customers]
  );

  const estimatedTotal = items.reduce((total, item) => {
    const product = productsById.get(item.productId);
    const quantity = Number(item.quantity);

    if (!product || Number.isNaN(quantity)) {
      return total;
    }

    return total + product.price * quantity;
  }, 0);

  const orderStats = useMemo(() => {
    return orders.reduce(
      (stats, order) => ({
        total: stats.total + 1,
        pending: stats.pending + (order.status === "Pending" ? 1 : 0),
        paid: stats.paid + (order.status === "Paid" ? 1 : 0),
        cancelled: stats.cancelled + (order.status === "Cancelled" ? 1 : 0),
      }),
      { total: 0, pending: 0, paid: 0, cancelled: 0 }
    );
  }, [orders]);

  const loadPageData = useCallback(async () => {
    try {
      setLoading(true);

      const [customersResult, productsResult, ordersResult] = await Promise.all([
        getCustomers(),
        getProducts(),
        getOrders(),
      ]);

      setCustomers(customersResult);
      setProducts(productsResult);
      setOrders(ordersResult);
      setCustomerId((currentCustomerId) => currentCustomerId || customersResult[0]?.id || "");
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to load order data.",
      });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadPageData();
  }, [loadPageData]);

  function updateItem(itemId: string, changes: Partial<OrderDraftItem>) {
    setItems((current) =>
      current.map((item) => (item.id === itemId ? { ...item, ...changes } : item))
    );
  }

  function removeItem(itemId: string) {
    setItems((current) => {
      if (current.length === 1) {
        return current;
      }

      return current.filter((item) => item.id !== itemId);
    });
  }

  function resetDraft() {
    setItems([createDraftItem()]);
  }

  function validateDraft() {
    if (!customerId) {
      return "Select a customer.";
    }

    if (items.length === 0) {
      return "Add at least one product.";
    }

    const productIds = items.map((item) => item.productId);

    if (productIds.some((productId) => !productId)) {
      return "Select a product for every item.";
    }

    if (new Set(productIds).size !== productIds.length) {
      return "Each product can only be added once.";
    }

    const invalidQuantity = items.some((item) => Number(item.quantity) <= 0);

    if (invalidQuantity) {
      return "Item quantity must be greater than zero.";
    }

    return "";
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);

    const validationMessage = validateDraft();

    if (validationMessage) {
      setFeedback({ type: "error", text: validationMessage });
      return;
    }

    try {
      setCreating(true);

      await createOrder({
        customerId,
        items: items.map((item) => ({
          productId: item.productId,
          quantity: Number(item.quantity),
        })),
      });

      resetDraft();
      setIsCreateModalOpen(false);
      setFeedback({ type: "success", text: "Order created successfully." });
      await loadPageData();
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to create order.",
      });
    } finally {
      setCreating(false);
    }
  }

  async function handlePay(orderId: string) {
    setFeedback(null);

    try {
      setProcessingOrderId(orderId);
      await payOrder(orderId);
      setFeedback({ type: "success", text: "Order paid successfully." });
      await loadPageData();
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to pay order.",
      });
    } finally {
      setProcessingOrderId("");
    }
  }

  async function handleCancel(orderId: string) {
    setFeedback(null);

    try {
      setProcessingOrderId(orderId);
      await cancelOrder(orderId);
      setFeedback({ type: "success", text: "Order cancelled successfully." });
      await loadPageData();
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to cancel order.",
      });
    } finally {
      setProcessingOrderId("");
    }
  }

  return (
    <div className="page-shell">
      <header className="page-header">
        <div>
          <p className="page-kicker">Order flow</p>
          <h1 className="page-title">Orders</h1>
        </div>

        <div className="button-row">
          <button
            type="button"
            onClick={() => {
              setFeedback(null);
              void loadPageData();
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
            New order
          </button>
        </div>
      </header>

      <section className="grid stats">
        <div className="panel stat-card">
          <span className="stat-label">Total orders</span>
          <strong className="stat-value">{orderStats.total}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Pending</span>
          <strong className="stat-value">{orderStats.pending}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Paid</span>
          <strong className="stat-value">{orderStats.paid}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Cancelled</span>
          <strong className="stat-value">{orderStats.cancelled}</strong>
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
            <h2 className="panel-title">Order list</h2>
            {loading && <span className="muted">Loading...</span>}
          </div>
        </div>

        {orders.length === 0 ? (
          <div className="empty-state">No orders found.</div>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Order</th>
                  <th>Created</th>
                  <th>Customer</th>
                  <th>Status</th>
                  <th className="numeric">Total</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((order) => {
                  const canProcess = order.status === "Pending";
                  const isProcessing = processingOrderId === order.id;
                  const customer = customersById.get(order.customerId);

                  return (
                    <tr key={order.id}>
                      <td>
                        <strong className="entity-name">#{getShortId(order.id)}</strong>
                      </td>
                      <td>{new Date(order.createdAtUtc).toLocaleString()}</td>
                      <td>
                        <strong className="entity-name">{customer?.name ?? "Unknown"}</strong>
                        <span className="entity-id">{getShortId(order.customerId)}</span>
                      </td>
                      <td>
                        <span className={`status-pill ${getStatusClass(order.status)}`}>
                          {order.status}
                        </span>
                      </td>
                      <td className="numeric">
                        <strong>{formatCurrency(order.totalAmount)}</strong>
                      </td>
                      <td>
                        <div className="button-row" style={{ justifyContent: "flex-start" }}>
                          <button
                            type="button"
                            disabled={!canProcess || isProcessing}
                            onClick={() => void handlePay(order.id)}
                            className="button success compact"
                          >
                            {isProcessing ? "..." : "Pay"}
                          </button>
                          <button
                            type="button"
                            disabled={!canProcess || isProcessing}
                            onClick={() => void handleCancel(order.id)}
                            className="button danger compact"
                          >
                            Cancel
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <Modal
        title="New order"
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      >
        <form onSubmit={handleSubmit} className="form-grid">
          <div className="page-header">
            <span className="muted">Estimated total: {formatCurrency(estimatedTotal)}</span>
            <button
              type="button"
              onClick={() => setItems((current) => [...current, createDraftItem()])}
              className="button secondary"
            >
              Add item
            </button>
          </div>

          <label className="field-label">
            Customer
            <select
              value={customerId}
              onChange={(event) => setCustomerId(event.target.value)}
              className="field"
              disabled={customers.length === 0}
              autoFocus
            >
              {customers.length === 0 ? (
                <option value="">No customers available</option>
              ) : (
                customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.name} ({customer.email})
                  </option>
                ))
              )}
            </select>
          </label>

          <div className="form-grid">
            {items.map((item, index) => {
              const selectedProduct = productsById.get(item.productId);
              const selectedProductIds = new Set(
                items.filter((current) => current.id !== item.id).map((current) => current.productId)
              );

              return (
                <div key={item.id} className="order-item-row">
                  <label className="field-label">
                    Product {index + 1}
                    <select
                      value={item.productId}
                      onChange={(event) => updateItem(item.id, { productId: event.target.value })}
                      className="field"
                      disabled={products.length === 0}
                    >
                      <option value="">Select product</option>
                      {products.map((product) => (
                        <option
                          key={product.id}
                          value={product.id}
                          disabled={selectedProductIds.has(product.id)}
                        >
                          {product.name} - {formatCurrency(product.price)} - stock {product.stockQuantity}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="field-label">
                    Quantity
                    <input
                      type="number"
                      min="1"
                      value={item.quantity}
                      onChange={(event) => updateItem(item.id, { quantity: event.target.value })}
                      className="field"
                    />
                  </label>

                  <div className="button-row" style={{ alignItems: "end", justifyContent: "flex-start" }}>
                    <strong style={{ minWidth: "82px" }}>
                      {formatCurrency((selectedProduct?.price ?? 0) * Number(item.quantity || 0))}
                    </strong>
                    <button
                      type="button"
                      onClick={() => removeItem(item.id)}
                      disabled={items.length === 1}
                      className="button secondary"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              );
            })}
          </div>

          <div className="button-row">
            <button type="button" onClick={resetDraft} className="button secondary">
              Clear
            </button>
            <button
              type="button"
              onClick={() => setIsCreateModalOpen(false)}
              className="button secondary"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={creating || customers.length === 0 || products.length === 0}
              className="button primary"
            >
              {creating ? "Creating..." : "Create order"}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
