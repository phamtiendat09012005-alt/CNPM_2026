import api from './api';

export const aiService = {
  async recordView(productId, sessionId) {
    const response = await api.post(`/recommendations/views/${productId}`, { sessionId });
    return response.data.data;
  },
  async related(productId, take = 4) {
    const response = await api.get(`/recommendations/related/${productId}`, { params: { take } });
    return response.data.data;
  },
  async personalized(take = 8) {
    const response = await api.get('/recommendations/personalized', { params: { take } });
    return response.data.data;
  },
  async recommendSize(payload) {
    const response = await api.post('/recommendations/size', payload);
    return response.data.data;
  },
};
