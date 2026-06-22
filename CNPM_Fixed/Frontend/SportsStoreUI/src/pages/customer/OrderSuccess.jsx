import { Link, useLocation } from 'react-router-dom';

export default function OrderSuccess() {
  const { state } = useLocation();
  const order = state?.order;

  return (
    <div className="container section narrow-section">
      <div className="success-card">
        <div className="success-icon">✓</div>
        <h1>Đặt hàng thành công</h1>
        <p>Đơn hàng của bạn đã được hệ thống ghi nhận.</p>
        {order && (
          <div className="order-code-box">
            <span>Mã đơn hàng</span>
            <strong>{order.orderCode}</strong>
          </div>
        )}
        <div className="button-row centered-row">
          {order && <Link className="btn btn-primary" to={`/orders/${order.id}`}>Xem đơn hàng</Link>}
          <Link className="btn btn-ghost" to="/products">Tiếp tục mua sắm</Link>
        </div>
      </div>
    </div>
  );
}
