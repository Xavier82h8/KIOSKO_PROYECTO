using System.Collections.Generic;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.BLL
{
    public class InventarioBLL
    {
        private InventarioDAL _inventarioDAL = new InventarioDAL();

        public List<Inventario> ObtenerInventario()
        {
            return _inventarioDAL.ObtenerInventario();
        }
    }
}
