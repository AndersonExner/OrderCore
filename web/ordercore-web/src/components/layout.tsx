import { Link, NavLink, Outlet } from "react-router-dom";
import NotificationsMenu from "./NotificationsMenu";

export default function Layout() {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header-inner">
          <Link to="/" className="brand-link" aria-label="OrderCore dashboard">
            <span className="brand-mark">OC</span>
            <span>
              <span className="brand-name">OrderCore</span>
              <span className="brand-subtitle">Order operations</span>
            </span>
          </Link>

          <div className="app-header-actions">
            <nav className="app-nav" aria-label="Main navigation">
              <NavLink to="/" end className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
                Home
              </NavLink>
              <NavLink
                to="/customers"
                className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
              >
                Customers
              </NavLink>
              <NavLink
                to="/products"
                className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
              >
                Products
              </NavLink>
              <NavLink
                to="/orders"
                className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
              >
                Orders
              </NavLink>
              <a
                href="https://localhost:7171/swagger"
                target="_blank"
                rel="noreferrer"
                className="nav-link"
              >
                Swagger
              </a>
            </nav>

            <NotificationsMenu />
          </div>
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
