import { useCallback, useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";

import Modal from "../components/Modal";
import { createProduct, getProducts } from "../api/products";
import type { ProductResponse } from "../api/products";

type Feedback = {
  type: "success" | "error";
  text: string;
};

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function getStockClass(stockQuantity: number) {
  if (stockQuantity <= 5) {
    return "low";
  }

  return "healthy";
}

function getStockLabel(stockQuantity: number) {
  if (stockQuantity <= 5) {
    return "Low stock";
  }

  return "Available";
}

export default function ProductsPage() {
  const [name, setName] = useState("");
  const [price, setPrice] = useState("");
  const [stockQuantity, setStockQuantity] = useState("");
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  const inventoryValue = useMemo(
    () =>
      products.reduce(
        (total, product) => total + product.price * product.stockQuantity,
        0
      ),
    [products]
  );

  const lowStockCount = useMemo(
    () => products.filter((product) => product.stockQuantity <= 5).length,
    [products]
  );

  const averagePrice = useMemo(() => {
    if (products.length === 0) {
      return 0;
    }

    return products.reduce((total, product) => total + product.price, 0) / products.length;
  }, [products]);

  const unitsInStock = useMemo(
    () => products.reduce((total, product) => total + product.stockQuantity, 0),
    [products]
  );

  const loadProducts = useCallback(async () => {
    try {
      setLoading(true);
      const result = await getProducts();
      setProducts(result);
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to load products.",
      });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);

    try {
      await createProduct({
        name,
        price: Number(price),
        stockQuantity: Number(stockQuantity),
      });

      setName("");
      setPrice("");
      setStockQuantity("");
      setIsCreateModalOpen(false);
      setFeedback({ type: "success", text: "Product created successfully." });
      await loadProducts();
    } catch (error) {
      setFeedback({
        type: "error",
        text: error instanceof Error ? error.message : "Failed to create product.",
      });
    }
  }

  return (
    <div className="page-shell">
      <header className="page-header">
        <div>
          <p className="page-kicker">Catalog</p>
          <h1 className="page-title">Products</h1>
        </div>

        <div className="button-row">
          <button
            type="button"
            onClick={() => {
              setFeedback(null);
              void loadProducts();
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
            New product
          </button>
        </div>
      </header>

      <section className="grid stats">
        <div className="panel stat-card">
          <span className="stat-label">Total products</span>
          <strong className="stat-value">{products.length}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Inventory value</span>
          <strong className="stat-value">{formatCurrency(inventoryValue)}</strong>
        </div>
        <div className="panel stat-card">
          <span className="stat-label">Low stock</span>
          <strong className="stat-value">{lowStockCount}</strong>
        </div>
      </section>

      {feedback && (
        <div className={`feedback ${feedback.type}`} role="status">
          {feedback.text}
        </div>
      )}

      <section className="panel panel-pad">
        <h2 className="panel-title">Catalog snapshot</h2>
        <div style={{ marginTop: "10px" }}>
          <div className="home-signal">
            <span className="muted">Average price</span>
            <strong>{formatCurrency(averagePrice)}</strong>
          </div>
          <div className="home-signal">
            <span className="muted">Units in stock</span>
            <strong>{unitsInStock}</strong>
          </div>
          <div className="home-signal">
            <span className="muted">Health</span>
            <span className={`status-pill ${lowStockCount > 0 ? "low" : "healthy"}`}>
              {lowStockCount > 0 ? "Needs attention" : "Balanced"}
            </span>
          </div>
        </div>
      </section>

      <section className="panel">
        <div className="panel-header">
          <div>
            <h2 className="panel-title">Product list</h2>
            {loading && <span className="muted">Loading...</span>}
          </div>
        </div>

        {products.length === 0 ? (
          <div className="empty-state">No products found.</div>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th className="numeric">Price</th>
                  <th className="numeric">Stock</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.id}>
                    <td>
                      <strong className="entity-name">{product.name}</strong>
                      <span className="entity-id">{product.id}</span>
                    </td>
                    <td className="numeric">{formatCurrency(product.price)}</td>
                    <td className="numeric">{product.stockQuantity}</td>
                    <td>
                      <span className={`status-pill ${getStockClass(product.stockQuantity)}`}>
                        {getStockLabel(product.stockQuantity)}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <Modal
        title="New product"
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      >
        <form onSubmit={handleSubmit} className="form-grid">
          <label className="field-label">
            Name
            <input
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="field"
              autoFocus
            />
          </label>

          <label className="field-label">
            Price
            <input
              type="number"
              step="0.01"
              min="0"
              value={price}
              onChange={(event) => setPrice(event.target.value)}
              className="field"
            />
          </label>

          <label className="field-label">
            Stock quantity
            <input
              type="number"
              min="0"
              value={stockQuantity}
              onChange={(event) => setStockQuantity(event.target.value)}
              className="field"
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
              Create product
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
