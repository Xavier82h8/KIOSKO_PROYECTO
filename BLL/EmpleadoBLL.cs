using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.BLL
{
    public class EmpleadoBLL
    {
        private EmpleadoDAL _empleadoDAL = new EmpleadoDAL();

        public Empleado ObtenerEmpleadoPorId(int id)
        {
            return _empleadoDAL.ObtenerEmpleadoPorId(id);
        }

        public int? ValidarCredenciales(string nombreEmp, string contrasena)
        {
            return _empleadoDAL.ValidarCredenciales(nombreEmp, contrasena);
        }

        public bool CambiarContrasena(int idEmpleado, string nuevaContrasena)
        {
            // En un futuro: aplicar hashing aqu�
            return _empleadoDAL.ActualizarContrasena(idEmpleado, nuevaContrasena);
        }
    }
}
