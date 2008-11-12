﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.Ninject;
using Northwind.Web.NinjectModules;
using SharpArch.Data.NHibernate;
using SharpArch.Web.NHibernate;
using System.Reflection;
using SharpArch.Web.Ninject;
using NHibernate.Cfg;
using FluentNHibernate;

namespace Northwind.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start() {
            InitializeNinject();
            RegisterRoutes(RouteTable.Routes);
        }

        private void InitializeNinject() {
            NinjectKernel.Initialize(new ControllersAutoBindModule("Northwind.Controllers"), new DataModule());
            ControllerBuilder.Current.SetControllerFactory(typeof(MvcContrib.Ninject.NinjectControllerFactory));
        }

        public override void Init() {
            base.Init();

            NHibernateSession.Init(new WebSessionStorage(this), 
                new string[] { Server.MapPath("./bin/Northwind.Data.dll") }, 
                Server.MapPath("Hibernate.cfg.xml"));
        }

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
            );
        }

        protected void Application_Error(object sender, EventArgs e) {
            // Useful for debugging
            Exception ex = Server.GetLastError();
            ReflectionTypeLoadException reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }
    }
}