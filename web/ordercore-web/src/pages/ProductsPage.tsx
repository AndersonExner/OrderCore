import { useEffect, useState } from "react";
import type { FormEvent } from "react";

import {
  createProduct,
  getProducts,
} from "../api/products";

import type { ProductResponse } from "../api/products";

export default function ProductsPage() {
  const [name, setName] = useState("");
  const [price, setPrice] = useState("");
  const [stockQuantity, setStockQuantity] = useState("");
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  async function loadProducts() {
    try {
      setLoading(true);
      const result = await getProducts();
      setProducts(result);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to load products.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadProducts();
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setMessage("");

    try {
      await createProduct({
        name,
        price: Number(price),
        stockQuantity: Number(stockQuantity),
      });

      setName("");
      setPrice("");
      setStockQuantity("");
      setMessage("Product created successfully.");
      await loadProducts();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Failed to create product.");
    }
  }

  return (
    <div>
      <h2>Products</h2>

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
          placeholder="Product name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <input
          placeholder="Price"
          type="number"
          step="0.01"
          value={price}
          onChange={(e) => setPrice(e.target.value)}
          style={{ padding: "10px", borderRadius: "8px", border: "1px solid #d1d5db" }}
        />

        <input
          placeholder="Stock quantity"
          type="number"
          value={stockQuantity}
          onChange={(e) => setStockQuantity(e.target.value)}
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
          Create product
        </button>
      </form>

      {message && <p>{message}</p>}

      <div>
        <h3>Product list</h3>
        {loading ? (
          <p>Loading...</p>
        ) : products.length === 0 ? (
          <p>No products found.</p>
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
                <th style={{ textAlign: "left", padding: "12px" }}>Price</th>
                <th style={{ textAlign: "left", padding: "12px" }}>Stock</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>{product.name}</td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>{product.price}</td>
                  <td style={{ padding: "12px", borderTop: "1px solid #e5e7eb" }}>{product.stockQuantity}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}