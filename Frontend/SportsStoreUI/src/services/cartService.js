import api from './api';

export const cartService = {
  async getCart() {
    const response = await api.get('/cart');
    return response.data.data;
  },
  async addItem(productVariantId, quantity = 1) {
    const response = await api.post('/cart/items', { productVariantId, quantity });
    return response.data.data;
  },
  async updateItem(itemId, quantity, isSelected = true) {
    const response = await api.put(`/cart/items/${itemId}`, { quantity, isSelected });
    return response.data.data;
  },
  async removeItem(itemId) {
    const response = await api.delete(`/cart/items/${itemId}`);
    return response.data.data;
  },
  async clear() {
    await api.delete('/cart');
  },
};
