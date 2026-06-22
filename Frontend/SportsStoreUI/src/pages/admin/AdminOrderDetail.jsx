import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import api, { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });
const nextStatus = {
  AwaitingPayment: 'Pending',
  Pending: 'Confirmed',
  Confirmed: 'Processing',
  Processing: 'Shipping',
  Shipping: 'Completed',
};

export default function AdminOrderDetail() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [error, setError] = useState('');

  const load = () => api.get(`/admin/orders/${id}`).then((r) => setOrder(r.data.data)).catch((e) => setError(getErrorMessage(e)));
  // load is intentionally recreated when the route id changes.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { load(); }, [id]);

  const changeStatus = async (status) => {
    try {
      await api.patch(`/admin/orders/${id}/status`, { status, note: 'Quản trị viên cập nhật trạng thái.' });
      await load();
    } catch (e) { setError(getErrorMessage(e)); }
  };

  if (!order) return <div className="state-card">Đang tải đơn hàng...</div>;

  return (
    <div>
      <div className="page-title-row">
        <div><div className="eyebrow">XỬ LÝ ĐƠN</div><h1>{order.orderCode}</h1></div>
        <span className={`status-badge status-${order.status.toLowerCase()}`}>{order.status}</span>
      </div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="order-detail-layout">
        <section className="form-card">
          <h3>Khách hàng và giao hàng</h3>
          <p><strong>{order.customerName}</strong></p>
          <p>{order.receiverName} · {order.receiverPhone}</p>
          <p>{order.shippingAddress}</p>
          <h3>Sản phẩm</h3>
          {order.items.map((item) => (
            <div className="order-line" key={item.id}>
              <div><strong>{item.productName}</strong><span>{item.sku} · {item.size || '-'} · {item.color || '-'}</span></div>
              <span>{item.quantity} × {money.format(item.unitPrice)}</span>
              <strong>{money.format(item.totalPrice)}</strong>
            </div>
          ))}
          <h3>Lịch sử</h3>
          <div className="timeline">{order.statusHistory.map((x) => <div key={x}>{x}</div>)}</div>
        </section>
        <aside className="summary-card">
          <h3>Thanh toán</h3>
          <div><span>Phương thức</span><strong>{order.payment?.method}</strong></div>
          <div><span>Trạng thái</span><strong>{order.payment?.status}</strong></div>
          <div className="summary-total"><span>Tổng cộng</span><strong>{money.format(order.totalAmount)}</strong></div>
          {nextStatus[order.status] && <button className="btn btn-primary btn-block" onClick={() => changeStatus(nextStatus[order.status])}>Chuyển sang {nextStatus[order.status]}</button>}
          {['Pending', 'Confirmed'].includes(order.status) && <button className="btn btn-danger btn-block" onClick={() => changeStatus('Cancelled')}>Hủy đơn</button>}
          <Link className="btn btn-ghost btn-block" to="/admin/orders">Quay lại</Link>
        </aside>
      </div>
    </div>
  );
}
