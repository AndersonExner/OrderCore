import { Link, Outlet } from "react-router-dom";

const navLinkStyle: React.CSSProperties = {
  color: "white",
  textDecoration: "none",
  fontWeight: 500,
};

export default function Layout() {
  return (
    <div style={{ fontFamily: "Arial, sans-serif", minHeight: "100vh", background: "#f5f7fb" }}>
      <header
        style={{
          background: "#111827",
          color: "white",
          padding: "16px 24px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <h1 style={{ margin: 0, fontSize: "20px" }}>OrderCore</h1>

        <nav style={{ display: "flex", gap: "16px" }}>
          <Link to="/" style={navLinkStyle}>Home</Link>
          <Link to="/customers" style={navLinkStyle}>Customers</Link>
          <Link to="/products" style={navLinkStyle}>Products</Link>
          <Link to="/orders" style={navLinkStyle}>Orders</Link>
          <a
            href="https://localhost:7171/swagger"
            target="_blank"
            rel="noreferrer"
            style={navLinkStyle}
          >
            Swagger
          </a>
        </nav>
      </header>

      <main style={{ maxWidth: "1100px", margin: "0 auto", padding: "24px" }}>
        <Outlet />
      </main>
    </div>
  );
}