import { useEffect, useState } from 'react';
import { authService } from '../../services/authService';

export default function Profile() {
  const [profile, setProfile] = useState(null);
  useEffect(() => { authService.me().then(setProfile); }, []);

  if (!profile) return <div className="container section"><div className="state-card">Đang tải hồ sơ...</div></div>;

  return (
    <div className="container section narrow-section">
      <div className="form-card">
        <div className="eyebrow">TÀI KHOẢN</div>
        <h1>Thông tin cá nhân</h1>
        <div className="profile-row"><span>Họ tên</span><strong>{profile.fullName}</strong></div>
        <div className="profile-row"><span>Email</span><strong>{profile.email}</strong></div>
        <div className="profile-row"><span>Số điện thoại</span><strong>{profile.phoneNumber || 'Chưa cập nhật'}</strong></div>
        <div className="profile-row"><span>Vai trò</span><strong>{profile.roles?.join(', ')}</strong></div>
        <div className="profile-row"><span>Trạng thái</span><strong>{profile.status}</strong></div>
      </div>
    </div>
  );
}
