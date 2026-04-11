import { getJson, postJson } from "./http";

export type CreateProductRequest = {
  name: string;
  price: number;
  stockQuantity: number;
};

export type ProductResponse = {
  id: string;
  name: string;
  price: number;
  stockQuantity: number;
};

export async function createProduct(request: CreateProductRequest) {
  return postJson<ProductResponse, CreateProductRequest>("/api/products", request);
}

export async function getProducts() {
  return getJson<ProductResponse[]>("/api/products");
}