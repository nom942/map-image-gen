namespace MapImageGen
{
    public class EventHandler
    {
        // method to call to send the generated map images to the web server




        /// Plan for the live position tracking
        // collect all alive players periodically (e.g. every 5 seconds)
        // group players in lists based on what zone they are in

        // list for lcz, hcz, ez and surface (can either make our own map of surface zone or find one)
        // store steam id and vector3 position of each player in their respective list

        // send each list as json package to the web server
        // 4 pages: lcz, hcz, ez and surface zone maps
        // map players as dots on the map of the zone that they are in

        // update the player dots with the new pos data every time the data is sent (e.g. every 5 seconds)


    }
}
