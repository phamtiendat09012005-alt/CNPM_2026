import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { orderService } from '../../services/orderService';
import { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function OrderList() {
  const [orders, setOrders] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    orderService.getMyOrders().then(setOrders).catch((e) => setError(getErrorMessage(e)));
  }, []);

  return (
    <div className="container section">
      <div className="page-title-row"><h1>Đơn hàng của tôi</h1></div>
      {error && <div className="alert alert-error">{error}</div>}
      {orders.length === 0 ? (
        <div className="state-card">Bạn chưa có đơn hàng nào.</div>
      ) : (
        <div className="table-card">
          <table>
            <thead><tr><th>Mã đơn</th><th>Ngày đặt</th><th>Sản phẩm</th><th>Tổng tiền</th><th>Thanh toán</th><th>Trạng thái</th><th></th></tr></thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td><strong>{order.orderCode}</strong></td>
                  <td>{new Date(order.createdAt).toLocaleString('vi-VN')}</td>
                  <td>{order.itemCount}</td>
                  <td>{money.format(order.totalAmount)}</td>
                  <td>{order.paymentStatus}</td>
                  <td><span className={`status-badge status-${order.status.toLowerCase()}`}>{order.status}</span></td>
                  <td><Link to={`/orders/${order.id}`}>Chi tiết</Link></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
