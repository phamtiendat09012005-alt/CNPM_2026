import api from './api';

export const orderService = {
  async checkout(payload) {
    const response = await api.post('/orders', payload);
    return response.data.data;
  },
  async simulatePayment(id) {
    const response = await api.post(`/orders/${id}/simulate-payment`);
    return response.data.data;
  },
  async getMyOrders() {
    const response = await api.get('/orders/my');
    return response.data.data;
  },
  async getOrder(id) {
    const response = await api.get(`/orders/${id}`);
    return response.data.data;
  },
  async cancelOrder(id) {
    const response = await api.post(`/orders/${id}/cancel`);
    return response.data.data;
  },
};
