
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
    public static class Data
    {
        public static RouteInfo GetRouteById(int id)
        {
            RouteInfo route = null;

            _routes.Value.TryGetValue(id, out route);

            return route;
        }

        private static Lazy<Dictionary<int, RouteInfo>> _routes = new Lazy<Dictionary<int, RouteInfo>>(() =>
        {
            return GetStaticData<RouteInfo>(StaticDataType.Routes, line =>
            {
                var elements = line.Split(',');

                return new RouteInfo
                {
                    Id = int.Parse(elements[0]),
                    ShortName = elements[1],
                    Name = elements[2]
                };
            })
            .ToDictionary(r => r.Id);
        });

        public static TripInfo GetTripById(int id)
        {
            TripInfo trip = null;

            _trips.Value.TryGetValue(id, out trip);

            return trip;
        }

        private static Lazy<Dictionary<int, TripInfo>> _trips = new Lazy<Dictionary<int, TripInfo>>(() =>
        {
            return GetStaticData<TripInfo>(StaticDataType.Trips, line =>
            {
                var elements = line.Split(',');

                return new TripInfo
                {
                    RouteId = int.Parse(elements[0]),
                    Id = int.Parse(elements[2]),
                    ShapeId = int.Parse(elements[6]),
                    Headsign = elements[3]
                };
            })
            .ToDictionary(t => t.Id);
        });

        public static StopInfo GetStopById(int id)
        {
            StopInfo stop = null;

            _stops.Value.TryGetValue(id, out stop);

            return stop;
        }

        private static Lazy<Dictionary<int, StopInfo>> _stops = new Lazy<Dictionary<int, StopInfo>>(() =>
        {
            return GetStaticData<StopInfo>(StaticDataType.Stops, line =>
            {
                var elements = line.Split(',');

                return new StopInfo
                {
                    Id = int.Parse(elements[0]),
                    Latitude = double.Parse(elements[3]),
                    Longitude = double.Parse(elements[4]),
                    Name = elements[2]
                };
            })
            .ToDictionary(s => s.Id);
        });

        public static IEnumerable<StopTimeInfo> GetStopTimesByStopId(int stopId)
        {
            StopTimeInfo[] times = null;

            _stoptimes.Value.Item1.TryGetValue(stopId, out times);

            return times ?? Enumerable.Empty<StopTimeInfo>();
        }

        public static IEnumerable<StopTimeInfo> GetStopTimesByTripId(int tripId)
        {
            StopTimeInfo[] times = null;

            _stoptimes.Value.Item2.TryGetValue(tripId, out times);

            return times ?? Enumerable.Empty<StopTimeInfo>();
        }

        public static IEnumerable<StopInfo> GetStopsByRouteId(int routeId)
        {
            var tripsForRoute = _trips.Value.Values.Where(t => t.RouteId == routeId)
                                     .Select(t => t.Id)
                                     .ToArray();

            return _stoptimes.Value.Item1.SelectMany(pair => pair.Value)
                                         .Join(tripsForRoute, st => st.TripId, tripId => tripId, (st, tripId) => st.StopId)
                                         .Distinct()
                                         .Select(stopId => GetStopById(stopId));
        }

        private static Lazy<Tuple<Dictionary<int, StopTimeInfo[]>, Dictionary<int, StopTimeInfo[]>>> _stoptimes =
            new Lazy<Tuple<Dictionary<int, StopTimeInfo[]>, Dictionary<int, StopTimeInfo[]>>>(() =>
            {
                var stoptimes = GetStaticData<StopTimeInfo>(StaticDataType.StopTimes, line =>
                {
                    var elements = line.Split(',');

                    return new StopTimeInfo
                    {
                        TripId = int.Parse(elements[0]),
                        Arrival = new TimeSpan(int.Parse(elements[1].Split(':')[0]), int.Parse(elements[1].Split(':')[1]), int.Parse(elements[1].Split(':')[2])),
                        Departure = new TimeSpan(int.Parse(elements[2].Split(':')[0]), int.Parse(elements[2].Split(':')[1]), int.Parse(elements[2].Split(':')[2])),
                        StopId = int.Parse(elements[3]),
                        Sequence = int.Parse(elements[4])
                    };
                })
                .ToArray();

                return Tuple.Create(
                    stoptimes.GroupBy(st => st.StopId).ToDictionary(g => g.Key, g => g.ToArray()),
                    stoptimes.GroupBy(st => st.TripId).ToDictionary(g => g.Key, g => g.ToArray()));
            });

        private static IEnumerable<T> GetStaticData<T>(StaticDataType type, Func<string, T> factory)
        {
            using (var strm = LoadCsvStream(type))
            using (var reader = new StreamReader(strm))
            {
                reader.ReadLine();  // skip header

                var line = reader.ReadLine();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    yield return factory(line);

                    line = reader.ReadLine();
                }
            }
        }

        private static Stream LoadCsvStream(StaticDataType type)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = string.Format("Common.static_data.{0}.txt", type.ToString().ToLower());

            return assembly.GetManifestResourceStream(resourceName);
        }
    }

    internal enum StaticDataType
    {
        Trips,
        Routes,
        Stops,
        StopTimes,
        Shapes
    }
}
