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

export type OrderListResponse = {
  id: string;
  customerId: string;
  status: string;
  createdAtUtc: string;
  totalAmount: number;
};

export async function createOrder(request: CreateOrderRequest) {
  return postJson<OrderResponse, CreateOrderRequest>("/api/orders", request);
}

export async function getOrders() {
  return getJson<OrderListResponse[]>("/api/orders");
}

export async function getOrderById(id: string) {
  return getJson<OrderResponse>(`/api/orders/${id}`);
}