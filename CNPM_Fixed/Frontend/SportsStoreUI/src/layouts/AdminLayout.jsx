import { NavLink, Outlet } from 'react-router-dom';
import Header from '../components/Header';

export default function AdminLayout() {
  return (
    <div className="app-shell">
      <Header />
      <div className="admin-shell container">
        <aside className="admin-sidebar">
          <h3>Quản trị</h3>
          <NavLink to="/admin" end>Tổng quan</NavLink>
          <NavLink to="/admin/products">Sản phẩm</NavLink>
          <NavLink to="/admin/categories">Danh mục</NavLink>
          <NavLink to="/admin/orders">Đơn hàng</NavLink>
        </aside>
        <main className="admin-content"><Outlet /></main>
      </div>
    </div>
  );
}
