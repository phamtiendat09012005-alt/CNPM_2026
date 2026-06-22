import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api, { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function OrderManagement() {
  const [orders, setOrders] = useState([]);
  const [status, setStatus] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/admin/orders', { params: status ? { status } : {} })
      .then((r) => setOrders(r.data.data))
      .catch((e) => setError(getErrorMessage(e)));
  }, [status]);

  return (
    <div>
      <div className="page-title-row">
        <h1>Đơn hàng</h1>
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          {['AwaitingPayment', 'Pending', 'Confirmed', 'Processing', 'Shipping', 'Completed', 'Cancelled'].map((x) => <option key={x}>{x}</option>)}
        </select>
      </div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="table-card">
        <table>
          <thead><tr><th>Mã đơn</th><th>Khách hàng</th><th>Ngày đặt</th><th>Tổng tiền</th><th>Thanh toán</th><th>Trạng thái</th><th></th></tr></thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id}>
                <td><strong>{order.orderCode}</strong></td>
                <td>{order.customerName}</td>
                <td>{new Date(order.createdAt).toLocaleString('vi-VN')}</td>
                <td>{money.format(order.totalAmount)}</td>
                <td>{order.paymentMethod} / {order.paymentStatus}</td>
                <td><span className={`status-badge status-${order.status.toLowerCase()}`}>{order.status}</span></td>
                <td><Link to={`/admin/orders/${order.id}`}>Xử lý</Link></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
