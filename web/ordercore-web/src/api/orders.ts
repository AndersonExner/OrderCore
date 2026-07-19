import { getJson, postJson } from "./http";

export type CreateOrderItemRequest = {
  productId: string;
  quantity: number;
};

export type CreateOrderRequest = {
  customerId: string;
  items: CreateOrderItemRequest[];
};

export type OrderItemResponse = {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  total: number;
};

export type OrderResponse = {
  id: string;
  customerId: string;
  status: string;
  createdAtUtc: string;
  totalAmount: number;
  items: OrderItemResponse[];
};

export type OrderSummaryResponse = Omit<OrderResponse, "items">;

export async function createOrder(request: CreateOrderRequest) {
  return postJson<OrderResponse, CreateOrderRequest>("/api/orders", request);
}

export async function getOrders() {
  return getJson<OrderSummaryResponse[]>("/api/orders");
}

export async function payOrder(id: string) {
  return postJson<OrderResponse, Record<string, never>>(`/api/orders/${id}/pay`, {});
}

export async function cancelOrder(id: string) {
  return postJson<OrderResponse, Record<string, never>>(`/api/orders/${id}/cancel`, {});
}
