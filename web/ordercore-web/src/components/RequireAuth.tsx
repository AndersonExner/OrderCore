import { Navigate, Outlet, useLocation } from "react-router-dom";

import { getAuthSession } from "../auth/session";

export default function RequireAuth() {
  const location = useLocation();
  const session = getAuthSession();

  if (!session) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
