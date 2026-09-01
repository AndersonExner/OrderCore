import { useState } from "react";
import type { FormEvent } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";

import { authRoles, login, registerUser } from "../api/auth";
import { getAuthSession, saveAuthSession } from "../auth/session";

type LoginMode = "login" | "register";
type LocationState = {
  from?: {
    pathname?: string;
  };
};

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const session = getAuthSession();
  const [mode, setMode] = useState<LoginMode>("login");
  const [userNameOrEmail, setUserNameOrEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<string>(authRoles.admin);
  const [feedback, setFeedback] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (session) {
    return <Navigate to="/" replace />;
  }

  const locationState = location.state as LocationState | null;
  const redirectPath = locationState?.from?.pathname ?? "/";

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setFeedback("");

    try {
      setIsSubmitting(true);

      const auth =
        mode === "login"
          ? await login({ userNameOrEmail, password })
          : await registerUser({ userName, email, password, role });

      saveAuthSession(auth);
      navigate(redirectPath, { replace: true });
    } catch (error) {
      setFeedback(error instanceof Error ? error.message : "Authentication failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-panel panel">
        <div className="auth-copy">
          <span className="brand-mark">OC</span>
          <p className="page-kicker">OrderCore</p>
          <h1 className="page-title">Sign in</h1>
          <p className="page-subtitle">
            Use a role-based account to access customers, products, orders, and payment
            notifications.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="form-grid auth-form">
          <div className="segmented-control" aria-label="Authentication mode">
            <button
              type="button"
              className={mode === "login" ? "active" : ""}
              onClick={() => setMode("login")}
            >
              Login
            </button>
            <button
              type="button"
              className={mode === "register" ? "active" : ""}
              onClick={() => setMode("register")}
            >
              Register
            </button>
          </div>

          {mode === "login" ? (
            <label className="field-label">
              User or email
              <input
                value={userNameOrEmail}
                onChange={(event) => setUserNameOrEmail(event.target.value)}
                className="field"
                autoFocus
              />
            </label>
          ) : (
            <>
              <label className="field-label">
                User name
                <input
                  value={userName}
                  onChange={(event) => setUserName(event.target.value)}
                  className="field"
                  autoFocus
                />
              </label>

              <label className="field-label">
                Email
                <input
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  className="field"
                />
              </label>

              <label className="field-label">
                Role
                <select value={role} onChange={(event) => setRole(event.target.value)} className="field">
                  <option value={authRoles.admin}>Admin</option>
                  <option value={authRoles.sales}>Sales</option>
                  <option value={authRoles.finance}>Finance</option>
                  <option value={authRoles.viewer}>Viewer</option>
                </select>
              </label>
            </>
          )}

          <label className="field-label">
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="field"
            />
          </label>

          {feedback && (
            <div className="feedback error" role="status">
              {feedback}
            </div>
          )}

          <button type="submit" className="button primary" disabled={isSubmitting}>
            {isSubmitting ? "Please wait..." : mode === "login" ? "Login" : "Create account"}
          </button>
        </form>
      </section>
    </main>
  );
}
