import { Link } from "react-router-dom";

const modules = [
  {
    label: "Customers",
    title: "Customer registry",
    detail: "Names, emails, and lookup flow.",
    to: "/customers",
  },
  {
    label: "Products",
    title: "Product catalog",
    detail: "Prices, stock, and availability.",
    to: "/products",
  },
  {
    label: "Orders",
    title: "Order lifecycle",
    detail: "Items, totals, payment, and cancellation.",
    to: "/orders",
  },
];

export default function HomePage() {
  return (
    <div className="page-shell">
      <header className="page-header">
        <div>
          <p className="page-kicker">Portfolio system</p>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-subtitle">
            A compact view over the modules already connected to the API, database, outbox,
            RabbitMQ flow, logs, and local notifications.
          </p>
        </div>

        <a
          href="https://localhost:7171/swagger"
          target="_blank"
          rel="noreferrer"
          className="button secondary"
        >
          Open Swagger
        </a>
      </header>

      <section className="home-grid">
        <div className="grid two">
          {modules.map((module) => (
            <Link key={module.to} to={module.to} className="panel home-card">
              <span className="status-pill neutral">{module.label}</span>
              <strong>{module.title}</strong>
              <span>{module.detail}</span>
            </Link>
          ))}
        </div>

        <aside className="panel panel-pad">
          <h2 className="panel-title">Project signals</h2>

          <div style={{ marginTop: "10px" }}>
            <div className="home-signal">
              <span className="muted">Backend</span>
              <span className="status-pill healthy">API ready</span>
            </div>
            <div className="home-signal">
              <span className="muted">Persistence</span>
              <span className="status-pill healthy">PostgreSQL</span>
            </div>
            <div className="home-signal">
              <span className="muted">Events</span>
              <span className="status-pill neutral">Outbox + RabbitMQ</span>
            </div>
            <div className="home-signal">
              <span className="muted">Observability</span>
              <span className="status-pill neutral">NLog</span>
            </div>
          </div>
        </aside>
      </section>
    </div>
  );
}
