import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Activos from './pages/Activos';
import TagsPage from './pages/Tags';
import Reportes from './pages/Reportes';
import Usuarios from './pages/Usuarios';
import Autorizaciones from './pages/Autorizaciones';
import Proveedores from './pages/Proveedores';
import Clientes from './pages/Clientes';
import Dispositivos from './pages/Dispositivos';
import Ubicaciones from './pages/Ubicaciones';
import Fabricantes from './pages/Fabricantes';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ThemeProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/activos" element={<Activos />} />
            <Route path="/tags" element={<TagsPage />} />
            <Route path="/reportes" element={<Reportes />} />
            <Route path="/usuarios" element={<Usuarios />} />
            <Route path="/autorizaciones" element={<Autorizaciones />} />
            <Route path="/proveedores" element={<Proveedores />} />
            <Route path="/clientes" element={<Clientes />} />
            <Route path="/dispositivos" element={<Dispositivos />} />
            <Route path="/ubicaciones" element={<Ubicaciones />} />
            <Route path="/fabricantes" element={<Fabricantes />} />
          </Route>
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
        </ThemeProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
