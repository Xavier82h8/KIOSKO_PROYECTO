using System.Collections.Generic;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.BLL
{
    public class InventarioBLL
    {
        private InventarioDAL _inventarioDAL = new InventarioDAL();

        public List<Inventario> ObtenerHistorialInventario()
        {
            return _inventarioDAL.ObtenerHistorialInventario();
        }

        public bool RegistrarEntrada(Inventario registro)
        {
            if (registro == null || registro.IdProducto <= 0 || registro.TotalProductos <= 0)
            {
                return false;
            }
            return _inventarioDAL.RegistrarEntrada(registro);
        }
    }
}
