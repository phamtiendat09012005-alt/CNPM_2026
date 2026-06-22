import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Header() {
  const { user, isAuthenticated, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <header className="site-header">
      <div className="container header-inner">
        <Link to="/" className="brand">SPORTS<span>AI</span></Link>
        <nav className="main-nav">
          <NavLink to="/" end>Trang chủ</NavLink>
          <NavLink to="/products">Sản phẩm</NavLink>
          {isAuthenticated && <NavLink to="/orders">Đơn hàng</NavLink>}
          {isAdmin && <NavLink to="/admin">Quản trị</NavLink>}
        </nav>
        <div className="header-actions">
          <Link className="icon-link" to="/cart">Giỏ hàng</Link>
          {isAuthenticated ? (
            <>
              <span className="user-name">{user.fullName}</span>
              <button className="btn btn-ghost btn-small" onClick={handleLogout}>Đăng xuất</button>
            </>
          ) : (
            <Link className="btn btn-primary btn-small" to="/login">Đăng nhập</Link>
          )}
        </div>
      </div>
    </header>
  );
}
