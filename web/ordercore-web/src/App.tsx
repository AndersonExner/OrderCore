import { BrowserRouter, Route, Routes } from "react-router-dom";
import Layout from "./components/layout";
import RequireAuth from "./components/RequireAuth";
import HomePage from "./pages/HomePage";
import CustomersPage from "./pages/CustomersPage";
import LoginPage from "./pages/LoginPage";
import OrdersPage from "./pages/OrdersPage";
import ProductsPage from "./pages/ProductsPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<RequireAuth />}>
          <Route path="/" element={<Layout />}>
            <Route index element={<HomePage />} />
            <Route path="customers" element={<CustomersPage />} />
            <Route path="products" element={<ProductsPage />} />
            <Route path="orders" element={<OrdersPage />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
