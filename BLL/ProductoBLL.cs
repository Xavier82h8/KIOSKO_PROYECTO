using System;
using System.Collections.Generic;
using System.Linq;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.BLL
{
    public class ProductoBLL
    {
        private ProductoDAL _productoDAL = new ProductoDAL();

        public List<Producto> ObtenerProductos() // Added for FormInventario
        {
            return _productoDAL.ObtenerProductos();
        }

        public List<Producto> ObtenerProductosDisponibles()
        {
            return _productoDAL.ObtenerProductos().Where(p => p.CantidadDisponible > 0).ToList();
        }

        public List<string> ObtenerCategorias()
        {
            return _productoDAL.ObtenerProductos().Select(p => p.Categoria).Distinct().ToList();
        }

        public List<Producto> ObtenerTodosLosProductos()
        {
            return _productoDAL.ObtenerProductos();
        }

        public Producto ObtenerProductoPorId(int id)
        {
            return _productoDAL.ObtenerProductoPorId(id);
        }

        public List<Producto> FiltrarProductos(string textoBusqueda, string categoriaSeleccionada)
        {
            var productos = _productoDAL.ObtenerProductos(); 

            if (!string.IsNullOrWhiteSpace(textoBusqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre.IndexOf(textoBusqueda, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(categoriaSeleccionada) && categoriaSeleccionada != "Todas las categorías")
            {
                productos = productos.Where(p => p.Categoria == categoriaSeleccionada).ToList();
            }

            return productos;
        }

        public void AgregarProducto(Producto producto)
        {
            // Aquí se podría añadir lógica de negocio adicional antes de agregar
            _productoDAL.AgregarProducto(producto);
        }

        public void ActualizarProducto(Producto producto)
        {
            // Aquí se podría añadir lógica de negocio adicional antes de actualizar
            _productoDAL.ActualizarProducto(producto);
        }

        public void EliminarProducto(int idProducto)
        {
            // Aquí se podría añadir lógica de negocio adicional antes de eliminar
            _productoDAL.EliminarProducto(idProducto);
        }
    }
}
