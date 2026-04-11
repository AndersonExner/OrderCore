import { Link } from "react-router-dom";

const cardStyle: React.CSSProperties = {
  background: "white",
  borderRadius: "12px",
  padding: "24px",
  boxShadow: "0 4px 14px rgba(0,0,0,0.08)",
  textDecoration: "none",
  color: "#111827",
};

export default function HomePage() {
  return (
    <div>
      <h2>Dashboard</h2>
      <p>Frontend for OrderCore modules.</p>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
          gap: "20px",
          marginTop: "24px",
        }}
      >
        <Link to="/customers" style={cardStyle}>
          <h3>Customers</h3>
          <p>Create customers.</p>
        </Link>

        <Link to="/products" style={cardStyle}>
          <h3>Products</h3>
          <p>Create and list products.</p>
        </Link>

        <Link to="/orders" style={cardStyle}>
          <h3>Orders</h3>
          <p>Create and list orders.</p>
        </Link>

        <a
          href="https://localhost:7171/swagger"
          target="_blank"
          rel="noreferrer"
          style={cardStyle}
        >
          <h3>Swagger</h3>
          <p>Open API documentation.</p>
        </a>
      </div>
    </div>
  );
}