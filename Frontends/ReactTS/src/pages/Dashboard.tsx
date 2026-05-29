import { useEffect, useState, useCallback, useRef } from 'react';
import api from '../services/api';
import { useSignalR } from '../hooks/useSignalR';
import type { KpiData, Movimiento, Tendencia } from '../types';
import {
  Package, MapPin, Tags, ArrowUpRight, ArrowDownRight, AlertTriangle, Wifi, WifiOff
} from 'lucide-react';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, LineChart, Line, CartesianGrid, Legend
} from 'recharts';

const COLORS = ['#3b82f6', '#22c55e', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

export default function Dashboard() {
  const [kpis, setKpis] = useState<KpiData | null>(null);
  const [tendencias, setTendencias] = useState<Tendencia[]>([]);
  const [movimientos, setMovimientos] = useState<Movimiento[]>([]);

  const kpisRef = useRef(kpis);
  kpisRef.current = kpis;

  const loadKpis = useCallback(async () => {
    try {
      const { data } = await api.get('/api/dashboard/kpis');
      if (data.success) setKpis(data.data);
    } catch { /* ignore */ }
  }, []);

  const loadTendencias = useCallback(async () => {
    try {
      const { data } = await api.get('/api/dashboard/tendencias');
      if (data.success) setTendencias(data.data.dias);
    } catch { /* ignore */ }
  }, []);

  const loadMovimientos = useCallback(async () => {
    try {
      const { data } = await api.get('/api/movimientos?page=1&pageSize=20');
      if (data.success) setMovimientos(data.data.items);
    } catch { /* ignore */ }
  }, []);

  const onNuevoMovimiento = useCallback((data: unknown) => {
    const m = data as Movimiento;
    setMovimientos(prev => [m, ...prev].slice(0, 100));
    loadKpis();
    loadTendencias();
  }, [loadKpis, loadTendencias]);

  const { connected } = useSignalR({ onMovimiento: onNuevoMovimiento });

  useEffect(() => {
    loadKpis();
    loadTendencias();
    loadMovimientos();
  }, [loadKpis, loadTendencias, loadMovimientos]);

  const kpiCards = kpis ? [
    { label: 'Total Activos', value: kpis.totalActivos, icon: Package, color: 'bg-blue-500' },
    { label: 'Activos Dentro', value: kpis.activosDentro, icon: MapPin, color: 'bg-green-500' },
    { label: 'Activos Fuera', value: kpis.activosFuera, icon: MapPin, color: 'bg-yellow-500' },
    { label: 'Tags Registrados', value: kpis.tagsRegistrados, icon: Tags, color: 'bg-purple-500' },
    { label: 'Tags Asignados', value: kpis.tagsAsignados, icon: Tags, color: 'bg-indigo-500' },
    { label: 'Tags Disponibles', value: kpis.tagsDisponibles, icon: Tags, color: 'bg-teal-500' },
    { label: 'Ingresos Hoy', value: kpis.ingresosHoy, icon: ArrowUpRight, color: 'bg-green-600' },
    { label: 'Salidas Hoy', value: kpis.salidasHoy, icon: ArrowDownRight, color: 'bg-red-500' },
  ] : [];

  const pieData = kpis ? [
    { name: 'Dentro', value: kpis.activosDentro },
    { name: 'Fuera', value: kpis.activosFuera },
  ] : [];

  const barData = kpis ? [
    { name: 'Tags', Registrados: kpis.tagsRegistrados, Asignados: kpis.tagsAsignados, Disponibles: kpis.tagsDisponibles },
  ] : [];

  const catData = kpis?.categorias?.map(c => ({ name: c.categoria, value: c.count })) ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Dashboard</h2>
        <div className="flex items-center gap-2 text-sm">
          {connected ? (
            <span className="flex items-center gap-1 text-green-600 dark:text-green-400"><Wifi size={14} /> Tiempo real</span>
          ) : (
            <span className="flex items-center gap-1 text-gray-400 dark:text-gray-500"><WifiOff size={14} /> Desconectado</span>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {kpiCards.map(kpi => (
          <div key={kpi.label} className="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs text-gray-500 dark:text-gray-400 uppercase">{kpi.label}</p>
                <p className="text-2xl font-bold text-gray-800 dark:text-gray-100">{kpi.value}</p>
              </div>
              <div className={`${kpi.color} p-2 rounded-lg`}>
                <kpi.icon size={20} className="text-white" />
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
          <h3 className="font-semibold text-gray-800 dark:text-gray-100 mb-4">Tags por Estado</h3>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={barData}>
              <XAxis dataKey="name" />
              <YAxis allowDecimals={false} />
              <Tooltip />
              <Bar dataKey="Registrados" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Asignados" fill="#6366f1" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Disponibles" fill="#14b8a6" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
          <h3 className="font-semibold text-gray-800 dark:text-gray-100 mb-4">Activos por Ubicación</h3>
          <ResponsiveContainer width="100%" height={250}>
            <PieChart>
              <Pie data={pieData} cx="50%" cy="50%" innerRadius={60} outerRadius={100}
                paddingAngle={4} dataKey="value" label={({ name, percent }: any) =>
                  `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                {pieData.map((_, i) => <Cell key={i} fill={COLORS[i]} />)}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4 lg:col-span-2">
          <h3 className="font-semibold text-gray-800 dark:text-gray-100 mb-4">Tendencia de Movimientos (7 días)</h3>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={tendencias}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="dia" />
              <YAxis allowDecimals={false} />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="ingresos" stroke="#22c55e" strokeWidth={2}
                name="Ingresos" dot={{ r: 4 }} />
              <Line type="monotone" dataKey="salidas" stroke="#ef4444" strokeWidth={2}
                name="Salidas" dot={{ r: 4 }} />
              <Line type="monotone" dataKey="noAutorizadas" stroke="#f59e0b" strokeWidth={2}
                name="No Autorizadas" dot={{ r: 4 }} strokeDasharray="5 5" />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {catData.length > 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4 lg:col-span-2">
            <h3 className="font-semibold text-gray-800 dark:text-gray-100 mb-4">Activos por Categoría</h3>
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={catData} layout="vertical">
                <XAxis type="number" allowDecimals={false} />
                <YAxis type="category" dataKey="name" width={150} />
                <Tooltip />
                <Bar dataKey="value" fill="#3b82f6" radius={[0, 4, 4, 0]}>
                  {catData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-700">
          <h3 className="font-semibold text-gray-800 dark:text-gray-100">Movimientos Recientes</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="text-left px-4 py-2 text-gray-600 dark:text-gray-300">Hora</th>
                <th className="text-left px-4 py-2 text-gray-600 dark:text-gray-300">Activo</th>
                <th className="text-left px-4 py-2 text-gray-600 dark:text-gray-300">Tipo</th>
                <th className="text-left px-4 py-2 text-gray-600 dark:text-gray-300">Estado</th>
              </tr>
            </thead>
            <tbody>
              {movimientos.map(m => (
                <tr key={m.id} className={`border-t border-gray-100 dark:border-gray-700 ${m.alarmaActivada ? 'bg-red-50 dark:bg-red-900/20' : ''}`}>
                  <td className="px-4 py-2 text-gray-600 dark:text-gray-300">
                    {new Date(m.fechaRegistro).toLocaleTimeString()}
                  </td>
                  <td className="px-4 py-2">
                    <span className="font-medium">{m.activoPlaca}</span>
                    <span className="text-gray-500 dark:text-gray-400 ml-1">{m.activoNombre}</span>
                  </td>
                  <td className="px-4 py-2">
                    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium ${
                      m.tipoMovimiento === 'INGRESO'
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300'
                    }`}>
                      {m.tipoMovimiento === 'INGRESO' ? <ArrowUpRight size={12} /> : <ArrowDownRight size={12} />}
                      {m.tipoMovimiento}
                    </span>
                  </td>
                  <td className="px-4 py-2">
                    {m.alarmaActivada ? (
                      <span className="flex items-center gap-1 text-red-600 dark:text-red-400 font-medium">
                        <AlertTriangle size={14} /> No Autorizado
                      </span>
                    ) : (
                      <span className="text-green-600 dark:text-green-400">{m.autorizado ? 'Autorizado' : 'Pendiente'}</span>
                    )}
                  </td>
                </tr>
              ))}
              {movimientos.length === 0 && (
                <tr>
                  <td colSpan={4} className="text-center py-8 text-gray-400 dark:text-gray-500">No hay movimientos registrados</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
