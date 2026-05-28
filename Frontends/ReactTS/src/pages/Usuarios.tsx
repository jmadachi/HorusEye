import { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import { Plus, X } from 'lucide-react';

export default function Usuarios() {
  const [users, setUsers] = useState<Array<{ id: string; email: string; userName: string; roles: string[] }>>([]);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ email: '', password: '', userName: '', role: 'Usuario de Consulta' });
  const [error, setError] = useState('');
  const { hasRole } = useAuth();

  useEffect(() => { loadUsers(); }, []);

  const loadUsers = async () => {
    try {
      const { data } = await api.get('/api/auth/users');
      if (data.success) setUsers(data.data);
    } catch (err) {
      console.error('Error loading users:', err);
    }
  };

  const handleRegister = async () => {
    setError('');
    try {
      await api.post('/api/auth/register', form);
      setShowModal(false);
      setForm({ email: '', password: '', userName: '', role: 'Usuario de Consulta' });
      loadUsers();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr.response?.data?.message || 'Error al registrar usuario');
      } else {
        setError('Error al registrar usuario');
      }
    }
  };

  if (!hasRole('Usuario de Gestión')) {
    return <p className="text-gray-500 dark:text-gray-400">No tienes permisos para ver esta página.</p>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Usuarios del Sistema</h2>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition cursor-pointer"
        >
          <Plus size={18} /> Nuevo Usuario
        </button>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Usuario</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Email</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Rol</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="border-t border-gray-100 dark:border-gray-700">
                <td className="px-4 py-3 font-medium">{u.userName}</td>
                <td className="px-4 py-3">{u.email}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${
                    u.roles.includes('Usuario de Gestión')
                      ? 'bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-300'
                      : 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300'
                  }`}>
                    {u.roles.join(', ')}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-semibold text-lg">Registrar Usuario</h3>
              <button onClick={() => setShowModal(false)} className="cursor-pointer">
                <X size={20} />
              </button>
            </div>

            {error && (
              <div className="bg-red-50 dark:bg-red-900/30 text-red-700 dark:text-red-300 px-4 py-2 rounded mb-4 text-sm">{error}</div>
            )}

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Nombre de Usuario</label>
                <input
                  value={form.userName}
                  onChange={(e) => setForm({ ...form, userName: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Email</label>
                <input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Contraseña</label>
                <input
                  type="password"
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Rol</label>
                <select
                  value={form.role}
                  onChange={(e) => setForm({ ...form, role: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                >
                  <option value="Usuario de Consulta">Usuario de Consulta</option>
                  <option value="Usuario de Gestión">Usuario de Gestión</option>
                </select>
              </div>
              <div className="flex justify-end gap-3 pt-2">
                <button
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700 rounded cursor-pointer"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleRegister}
                  className="px-4 py-2 bg-[#1e3a5f] text-white rounded cursor-pointer"
                >
                  Registrar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
