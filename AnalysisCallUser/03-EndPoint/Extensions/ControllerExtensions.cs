using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AnalysisCallUser._03_EndPoint.Extensions
{

    public static class ControllerExtensions
    {
        /// <summary>
        /// Renders a partial view to string.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="controller">The controller to extend.</param>
        /// <param name="viewNamePath">The name of the partial view.</param>
        /// <param name="model">The model to pass to the view.</param>
        /// <returns>The rendered partial view as a string.</returns>
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewNamePath, TModel model, bool isPartial = false)
        {
            if (string.IsNullOrEmpty(viewNamePath))
            {
                viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, !isPartial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewNamePath} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
