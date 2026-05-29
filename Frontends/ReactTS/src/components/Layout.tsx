import { useState } from 'react';
import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import {
  LayoutDashboard, Package, Tags, FileText, LogOut, Menu, X, Eye, Shield, Users, Sun, Moon, ShieldCheck
} from 'lucide-react';

export default function Layout() {
  const { user, logout, hasRole } = useAuth();
  const { dark, toggle } = useTheme();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);

  const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/activos', label: 'Activos', icon: Package },
    { to: '/tags', label: 'Tags RFID', icon: Tags },
    { to: '/reportes', label: 'Reportes', icon: FileText },
    { to: '/autorizaciones', label: 'Autorizaciones', icon: ShieldCheck },
    ...(hasRole('Usuario de Gestión')
      ? [{ to: '/usuarios', label: 'Usuarios', icon: Users }]
      : [])
  ];

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="h-screen flex flex-col overflow-hidden bg-gray-100 dark:bg-gray-900">
      <header className="bg-[#1e3a5f] text-white shadow-lg">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center gap-3">
            <button
              className="lg:hidden p-1 hover:bg-[#2d5a8e] rounded"
              onClick={() => setMenuOpen(!menuOpen)}
            >
              {menuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
            <Eye size={28} className="text-[#f59e0b]" />
            <h1 className="text-xl font-bold">HorusEye</h1>
          </div>

          <div className="flex items-center gap-3">
            <button onClick={toggle}
              className="p-1.5 rounded-lg hover:bg-[#2d5a8e] transition"
              title={dark ? 'Modo claro' : 'Modo oscuro'}>
              {dark ? <Sun size={18} /> : <Moon size={18} />}
            </button>
            <div className="flex items-center gap-2">
              {hasRole('Usuario de Gestión')
                ? <Shield size={16} className="text-[#f59e0b]" />
                : <Eye size={16} className="text-green-400" />
              }
              <span className="text-sm hidden sm:inline">{user?.userName}</span>
            </div>
            <button
              onClick={logout}
              className="flex items-center gap-1 text-sm hover:text-red-300 dark:hover:text-red-400 transition"
            >
              <LogOut size={16} />
              <span className="hidden sm:inline">Salir</span>
            </button>
          </div>
        </div>

        <nav className="hidden lg:flex bg-[#2d5a8e] px-4">
          <div className="flex gap-1">
            {navItems.map(({ to, label, icon: Icon }) => (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-2 px-4 py-2 text-sm transition ${
                  isActive(to)
                    ? 'bg-[#1e3a5f] text-white border-b-2 border-[#f59e0b]'
                    : 'text-gray-200 dark:text-gray-300 hover:bg-[#1e3a5f] hover:text-white'
                }`}
              >
                <Icon size={16} />
                {label}
              </Link>
            ))}
          </div>
        </nav>
      </header>

      {menuOpen && (
        <div className="lg:hidden bg-[#2d5a8e] border-t border-[#1e3a5f]">
          <div className="flex flex-col">
            {navItems.map(({ to, label, icon: Icon }) => (
              <Link
                key={to}
                to={to}
                onClick={() => setMenuOpen(false)}
                className={`flex items-center gap-2 px-4 py-3 text-sm transition ${
                  isActive(to)
                    ? 'bg-[#1e3a5f] text-white border-l-4 border-[#f59e0b]'
                    : 'text-gray-200 dark:text-gray-300 hover:bg-[#1e3a5f]'
                }`}
              >
                <Icon size={16} />
                {label}
              </Link>
            ))}
          </div>
        </div>
      )}

      <main className="flex-1 p-4 lg:p-6 overflow-auto">
        <Outlet />
      </main>

      <footer className="bg-[#1e3a5f] text-gray-400 text-center text-xs py-3">
        Todos los Derechos Reservados 2026
      </footer>
    </div>
  );
}
