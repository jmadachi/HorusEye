export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  role: string;
  userName: string;
}

export interface User {
  id: string;
  email: string;
  userName: string;
  role: string;
}

export interface Activo {
  id: string;
  placa: string;
  nombre: string;
  categoria: string;
  tenedorResponsable: string | null;
  estadoUbicacion: string;
  tagId: string | null;
  fechaRegistro: string;
}

export interface Tag {
  id: string;
  estado: string;
  fechaRegistro: string;
  fechaActualizacion: string;
}

export interface Movimiento {
  id: number;
  activoId: string;
  activoPlaca: string;
  activoNombre: string;
  puntoLecturaId: string | null;
  tipoMovimiento: string;
  autorizado: boolean;
  alarmaActivada: boolean;
  fechaRegistro: string;
}

export interface KpiData {
  totalActivos: number;
  activosDentro: number;
  activosFuera: number;
  tagsRegistrados: number;
  tagsAsignados: number;
  tagsDisponibles: number;
  ingresosHoy: number;
  salidasHoy: number;
  categorias: { categoria: string; count: number }[];
  fecha: string;
}

export interface Tendencia {
  fecha: string;
  dia: string;
  ingresos: number;
  salidas: number;
  noAutorizadas: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[] | null;
  timestamp: string;
}

export interface EventoRfidResponse {
  tagId: string;
  activoPlaca: string | null;
  activoNombre: string | null;
  tipoMovimiento: string;
  autorizado: boolean;
  activarAlarmaSonora: boolean;
  mensaje: string;
  timestamp: string;
}
