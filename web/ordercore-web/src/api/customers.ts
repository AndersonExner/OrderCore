import { getJson, postJson } from "./http";

export type CreateCustomerRequest = {
  name: string;
  email: string;
};

export type CustomerResponse = {
  id: string;
  name: string;
  email: string;
};

export async function createCustomer(request: CreateCustomerRequest) {
  return postJson<CustomerResponse, CreateCustomerRequest>("/api/customers", request);
}

export async function getCustomerById(id: string) {
  return getJson<CustomerResponse>(`/api/customers/${id}`);
}

export async function getCustomers() {
  return getJson<CustomerResponse[]>("/api/customers");
}