import { Outlet } from 'react-router-dom';
import Header from '../components/Header';

export default function CustomerLayout() {
  return (
    <div className="app-shell">
      <Header />
      <main className="page-content"><Outlet /></main>
      <footer className="site-footer">
        <div className="container">Sports Store AI · Đồ án Công nghệ phần mềm</div>
      </footer>
    </div>
  );
}
