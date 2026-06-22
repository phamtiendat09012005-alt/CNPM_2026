import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { orderService } from '../../services/orderService';
import { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function OrderDetail() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [error, setError] = useState('');

  const load = () => orderService.getOrder(id).then(setOrder).catch((e) => setError(getErrorMessage(e)));
  // load is intentionally recreated when the route id changes.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { load(); }, [id]);

  const cancel = async () => {
    if (!window.confirm('Bạn chắc chắn muốn hủy đơn hàng?')) return;
    try {
      await orderService.cancelOrder(id);
      await load();
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  if (!order) return <div className="container section"><div className="state-card">Đang tải đơn hàng...</div></div>;
  const canCancel = ['AwaitingPayment', 'Pending', 'Confirmed'].includes(order.status);

  return (
    <div className="container section">
      <div className="page-title-row">
        <div><div className="eyebrow">ĐƠN HÀNG</div><h1>{order.orderCode}</h1></div>
        <span className={`status-badge status-${order.status.toLowerCase()}`}>{order.status}</span>
      </div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="order-detail-layout">
        <section className="form-card">
          <h3>Sản phẩm</h3>
          {order.items.map((item) => (
            <div className="order-line" key={item.id}>
              <div><strong>{item.productName}</strong><span>{item.sku} · {item.size || '-'} · {item.color || '-'}</span></div>
              <span>{item.quantity} × {money.format(item.unitPrice)}</span>
              <strong>{money.format(item.totalPrice)}</strong>
            </div>
          ))}
          <h3>Lịch sử trạng thái</h3>
          <div className="timeline">
            {order.statusHistory.map((item) => <div key={item}>{item}</div>)}
          </div>
        </section>
        <aside className="summary-card">
          <h3>Thông tin giao hàng</h3>
          <p><strong>{order.receiverName}</strong></p>
          <p>{order.receiverPhone}</p>
          <p>{order.shippingAddress}</p>
          <h3>Thanh toán</h3>
          <div><span>Phương thức</span><strong>{order.payment?.method}</strong></div>
          <div><span>Trạng thái</span><strong>{order.payment?.status}</strong></div>
          <div><span>Tạm tính</span><strong>{money.format(order.subtotal)}</strong></div>
          <div><span>Phí giao</span><strong>{money.format(order.shippingFee)}</strong></div>
          <div className="summary-total"><span>Tổng</span><strong>{money.format(order.totalAmount)}</strong></div>
          {canCancel && <button className="btn btn-danger btn-block" onClick={cancel}>Hủy đơn hàng</button>}
          <Link className="btn btn-ghost btn-block" to="/orders">Quay lại danh sách</Link>
        </aside>
      </div>
    </div>
  );
}
