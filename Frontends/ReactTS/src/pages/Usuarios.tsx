import { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import { Plus, X, Pencil, Trash2, KeyRound } from 'lucide-react';

interface User {
  id: string;
  email: string;
  userName: string;
  roles: string[];
}

type ModalType = 'register' | 'edit' | 'reset-password' | null;

export default function Usuarios() {
  const [users, setUsers] = useState<User[]>([]);
  const [modalType, setModalType] = useState<ModalType>(null);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [form, setForm] = useState({ email: '', password: '', userName: '', role: 'Usuario de Consulta' });
  const [resetPassword, setResetPassword] = useState('');
  const [error, setError] = useState('');
  const { hasRole, user: currentUser } = useAuth();

  useEffect(() => { loadUsers(); }, []);

  const loadUsers = async () => {
    try {
      const { data } = await api.get('/api/auth/users');
      if (data.success) setUsers(data.data);
    } catch { /* ignore */ }
  };

  const openRegister = () => {
    setSelectedUser(null);
    setForm({ email: '', password: '', userName: '', role: 'Usuario de Consulta' });
    setError('');
    setModalType('register');
  };

  const openEdit = (u: User) => {
    setSelectedUser(u);
    setForm({ email: u.email, password: '', userName: u.userName, role: u.roles[0] || 'Usuario de Consulta' });
    setError('');
    setModalType('edit');
  };

  const openResetPassword = (u: User) => {
    setSelectedUser(u);
    setResetPassword('');
    setError('');
    setModalType('reset-password');
  };

  const handleRegister = async () => {
    setError('');
    try {
      await api.post('/api/auth/register', form);
      setModalType(null);
      loadUsers();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Error al registrar usuario');
    }
  };

  const handleEdit = async () => {
    if (!selectedUser) return;
    setError('');
    try {
      await api.put(`/api/auth/users/${selectedUser.id}`, {
        userName: form.userName,
        email: form.email,
        role: form.role
      });
      setModalType(null);
      loadUsers();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Error al actualizar usuario');
    }
  };

  const handleResetPassword = async () => {
    if (!selectedUser) return;
    setError('');
    try {
      await api.post(`/api/auth/users/${selectedUser.id}/reset-password`, {
        newPassword: resetPassword
      });
      setModalType(null);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Error al restablecer contraseña');
    }
  };

  const handleDelete = async (u: User) => {
    if (!confirm(`¿Eliminar usuario "${u.userName}" (${u.email})?`)) return;
    try {
      await api.delete(`/api/auth/users/${u.id}`);
      loadUsers();
    } catch { /* ignore */ }
  };

  if (!hasRole('Usuario de Gestión')) {
    return <p className="text-gray-500 dark:text-gray-400">No tienes permisos para ver esta página.</p>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Usuarios del Sistema</h2>
        <button onClick={openRegister}
          className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition cursor-pointer">
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
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="border-t border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700">
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
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button onClick={() => openEdit(u)}
                      className="p-1.5 text-blue-600 hover:bg-blue-100 rounded dark:text-blue-400 dark:hover:bg-blue-900/40 cursor-pointer"
                      title="Editar">
                      <Pencil size={14} />
                    </button>
                    <button onClick={() => openResetPassword(u)}
                      className="p-1.5 text-amber-600 hover:bg-amber-100 rounded dark:text-amber-400 dark:hover:bg-amber-900/40 cursor-pointer"
                      title="Resetear contraseña">
                      <KeyRound size={14} />
                    </button>
                    {currentUser?.id !== u.id && (
                      <button onClick={() => handleDelete(u)}
                        className="p-1.5 text-red-600 hover:bg-red-100 rounded dark:text-red-400 dark:hover:bg-red-900/40 cursor-pointer"
                        title="Eliminar">
                        <Trash2 size={14} />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Register Modal */}
      {modalType === 'register' && <UserFormModal
        title="Registrar Usuario"
        form={form} setForm={setForm}
        error={error} onSave={handleRegister} onCancel={() => setModalType(null)}
        saveLabel="Registrar" showPassword
      />}

      {/* Edit Modal */}
      {modalType === 'edit' && <UserFormModal
        title="Editar Usuario"
        form={form} setForm={setForm}
        error={error} onSave={handleEdit} onCancel={() => setModalType(null)}
        saveLabel="Guardar" showPassword={false}
      />}

      {/* Reset Password Modal */}
      {modalType === 'reset-password' && selectedUser && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-semibold text-lg">Resetear Contraseña</h3>
              <button onClick={() => setModalType(null)} className="cursor-pointer"><X size={20} /></button>
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Nueva contraseña para <strong>{selectedUser.userName}</strong> ({selectedUser.email})
            </p>
            {error && <div className="bg-red-50 dark:bg-red-900/30 text-red-700 dark:text-red-300 px-4 py-2 rounded mb-4 text-sm">{error}</div>}
            <input
              type="password"
              value={resetPassword}
              onChange={(e) => setResetPassword(e.target.value)}
              placeholder="Nueva contraseña (mín. 6 caracteres)"
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
            />
            <div className="flex justify-end gap-3">
              <button onClick={() => setModalType(null)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700 rounded cursor-pointer">
                Cancelar
              </button>
              <button onClick={handleResetPassword}
                className="px-4 py-2 bg-amber-600 text-white rounded cursor-pointer">
                Restablecer
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function UserFormModal({
  title, form, setForm, error, onSave, onCancel, saveLabel, showPassword
}: {
  title: string;
  form: { email: string; password: string; userName: string; role: string };
  setForm: (f: typeof form) => void;
  error: string;
  onSave: () => void;
  onCancel: () => void;
  saveLabel: string;
  showPassword: boolean;
}) {
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-semibold text-lg">{title}</h3>
          <button onClick={onCancel} className="cursor-pointer"><X size={20} /></button>
        </div>
        {error && (
          <div className="bg-red-50 dark:bg-red-900/30 text-red-700 dark:text-red-300 px-4 py-2 rounded mb-4 text-sm">{error}</div>
        )}
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1 dark:text-gray-200">Nombre de Usuario</label>
            <input value={form.userName}
              onChange={(e) => setForm({ ...form, userName: e.target.value })}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1 dark:text-gray-200">Email</label>
            <input type="email" value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]" />
          </div>
          {showPassword && (
            <div>
              <label className="block text-sm font-medium mb-1 dark:text-gray-200">Contraseña</label>
              <input type="password" value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]" />
            </div>
          )}
          <div>
            <label className="block text-sm font-medium mb-1 dark:text-gray-200">Rol</label>
            <select value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]">
              <option value="Usuario de Consulta">Usuario de Consulta</option>
              <option value="Usuario de Gestión">Usuario de Gestión</option>
            </select>
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button onClick={onCancel}
              className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700 rounded cursor-pointer">
              Cancelar
            </button>
            <button onClick={onSave}
              className="px-4 py-2 bg-[#1e3a5f] text-white rounded cursor-pointer">
              {saveLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
