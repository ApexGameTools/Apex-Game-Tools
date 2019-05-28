/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using Apex.LoadBalancing;

    /// <summary>
    /// Creating additional load balancers is a simple matter of deriving a class from LoadBalancer, and then defining one or more additional load balancers.
    /// </summary>
    public class ExamplesLoadBalancer : LoadBalancer
    {
        //This is how you define an additional load balancer using a static field
        public static readonly ILoadBalancer examplesBalancer = new LoadBalancedQueue(10, 1f, 20, 4);

        //An alternative way is by using static properties. In this example it will be initialized with a load balancer using default settings.
        public static ILoadBalancer extraBalancer
        {
            get;
            set;
        }
    }
}
