import api from './api';

export const productService = {
  async getProducts(params = {}) {
    const response = await api.get('/products', { params });
    return response.data.data;
  },
  async getProduct(id) {
    const response = await api.get(`/products/${id}`);
    return response.data.data;
  },
  async getCategories() {
    const response = await api.get('/categories');
    return response.data.data;
  },
  async getBrands() {
    const response = await api.get('/brands');
    return response.data.data;
  },
};
