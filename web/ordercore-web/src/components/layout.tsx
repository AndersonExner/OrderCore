import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { clearAuthSession, getCurrentUser } from "../auth/session";
import NotificationsMenu from "./NotificationsMenu";

export default function Layout() {
  const navigate = useNavigate();
  const user = getCurrentUser();

  function handleLogout() {
    clearAuthSession();
    navigate("/login", { replace: true });
  }

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

            {user && (
              <div className="user-chip">
                <strong>{user.userName}</strong>
                <span>{user.role}</span>
              </div>
            )}

            <NotificationsMenu />

            <button type="button" className="button compact header-logout" onClick={handleLogout}>
              Logout
            </button>
          </div>
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
