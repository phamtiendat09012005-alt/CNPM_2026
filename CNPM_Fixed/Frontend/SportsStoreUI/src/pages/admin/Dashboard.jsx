import { useEffect, useState } from 'react';
import api from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function Dashboard() {
  const [stats, setStats] = useState({ products: 0, orders: 0, revenue: 0, pending: 0 });

  useEffect(() => {
    Promise.all([
      api.get('/admin/products'),
      api.get('/admin/orders'),
    ]).then(([productsResponse, ordersResponse]) => {
      const products = productsResponse.data.data || [];
      const orders = ordersResponse.data.data || [];
      setStats({
        products: products.length,
        orders: orders.length,
        revenue: orders.filter((x) => x.status === 'Completed').reduce((sum, x) => sum + x.totalAmount, 0),
        pending: orders.filter((x) => ['Pending', 'Confirmed', 'Processing'].includes(x.status)).length,
      });
    });
  }, []);

  return (
    <div>
      <div className="page-title-row"><div><div className="eyebrow">QUẢN TRỊ</div><h1>Tổng quan</h1></div></div>
      <div className="stats-grid">
        <div className="stat-card"><span>Sản phẩm</span><strong>{stats.products}</strong></div>
        <div className="stat-card"><span>Tổng đơn hàng</span><strong>{stats.orders}</strong></div>
        <div className="stat-card"><span>Đơn đang xử lý</span><strong>{stats.pending}</strong></div>
        <div className="stat-card"><span>Doanh thu hoàn thành</span><strong>{money.format(stats.revenue)}</strong></div>
      </div>
      <div className="form-card admin-welcome">
        <h2>Sports Store AI</h2>
        <p>Quản lý danh mục, sản phẩm, biến thể và xử lý đơn hàng tại đây.</p>
      </div>
    </div>
  );
}
