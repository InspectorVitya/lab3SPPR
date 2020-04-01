using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using OSMLSGlobalLibrary;
using OSMLSGlobalLibrary.Map;
using OSMLSGlobalLibrary.Modules;

namespace TestModule
{
public class TestModule : OSMLSModule
{


    static Timer timer;
    private readonly int _leftX = -18000000;
    private readonly int _rightX = -9700000;
    private readonly int _downY = -10000000;
    private readonly int _upY = -4200000;
    ObjResearch objRes;
    Submarine submarine;
    bool endResearch = true;
    Random rnd = new Random();

    

    protected override void Initialize()
    {
        // добавляем регион подводной лодки и подлодку
        MapObjects.Add(new Region(_leftX, _rightX, _upY, _downY).polygon);
        MapObjects.Add(new Submarine(new Coordinate(-14000000, -7000000), 5000));

        // создаем таймер на создание объектов для исследование каждые 5с
        Init();

        //Получим объект подлодки и объекта для исследования
        objRes = MapObjects.GetAll<ObjResearch>()[0];
        submarine = MapObjects.GetAll<Submarine>()[0];


           
    }


  



    public void Init()
    {
        timer = new Timer(NewObj, null, 0, 5000);
    }

    public void NewObj(object obj)
    {
        MapObjects.Add(new ObjResearch(new Coordinate(rnd.Next(_leftX, _rightX), rnd.Next(_downY, _upY)), rnd.Next(2000, 15000)));
    }



    public async Task MethodWithDelayAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);

        endResearch = true;
        MapObjects.Remove(objRes);
        Console.WriteLine("Исследование закончилось 😎😎😎😎");

        // ищем ближайщий объект  
        int minIndex = 0;
        double min = 0;
        int i = 0;
        var objOnMap = MapObjects.GetAll<ObjResearch>();
        foreach (var minDist in objOnMap)
        {
            if (min == 0)
            {
                min = TestModule.Distance(submarine, minDist);
                i++;
                continue;
            }

            if (min > TestModule.Distance(submarine, minDist))
            {
                min = TestModule.Distance(submarine, minDist);
                minIndex = i;
            }
            i++;
        }

        objRes = objOnMap[minIndex];

    }



    public static double Distance(Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));



    public override void Update(long elapsedMilliseconds)
    {
        if (submarine.InObj(objRes.coordinate))
        {
            if (endResearch)
            {
                endResearch = false;
                Console.WriteLine("Исследование " + objRes.timeResearch + " c");
                 //Ждем пока закончиться исследование
                _ = MethodWithDelayAsync(objRes.timeResearch);
                   
            }

        }else submarine.Move(objRes.coordinate); 



    }



}





[CustomStyle(
        @"new ol.style.Style({
        image: new ol.style.Circle({
            opacity: 1.0,
            scale: 1.0,
            radius: 3,
            fill: new ol.style.Fill({
                color: 'rgba(0, 255, 0, 0.4)'
            }),
            stroke: new ol.style.Stroke({
                color: 'rgba(0, 0, 0, 0.4)',
                width: 1
            }),
        })
    });
    ")]
    // объект исследования
    public class ObjResearch : Point
{
    public int timeResearch;
    public Coordinate coordinate;


    public ObjResearch(Coordinate coordinate, int time) : base(coordinate)
    {
        timeResearch = time;
        this.coordinate = coordinate;
    }

      
}



[CustomStyle(
    @"new ol.style.Style({
    image: new ol.style.Circle({
        opacity: 1.0,
        scale: 1.0,
        radius: 3,
        fill: new ol.style.Fill({
            color: 'rgba(32, 250, 12, 1);'
        }),
        stroke: new ol.style.Stroke({
            color: 'rgba(0, 0, 0, 0.7)',
            width: 10
        }),
    })
});
")]
public class Submarine : Point
{
    public double speed { get; set; }
    public Coordinate coordinate;
    public Submarine(Coordinate coordinate, double sp) : base(coordinate)
    {
        speed = sp;
        this.coordinate = coordinate;
    }


    public void Move(Coordinate obj)
    {
        //  Движение по прямой к объекту исследование 

        var x1 = X;
        var y1 = Y;
        var x2 = obj.X;
        var y2 = obj.Y;
        var x = X;
            
        if (x1 < x2)
        {
            x += speed;
            if((x2-x1) < speed)
            {
                x += x2 - x1;
            }
        }
        if (x1 > x2)
        {
            x -= speed;
            if ((x1 - x2) < speed)
            {
                x += x2 - x1;
            }
        }



        X = x;
        Y = ((y2 * (x - x1)) - (y1 * (x - x2))) / (x2 - x1);
          
    }

     //Проверяем находится подлодка на точке объекта
    public bool InObj(Coordinate obj)
    {
        if (coordinate.X < obj.X)
        {
            return false;
        }
        if (coordinate.X > obj.X)
        {
            return false;
        }
        if (coordinate.Y < obj.Y)
        {
            return false;
        }
        if (coordinate.Y > obj.Y)
        {
            return false;
        }
        if (coordinate.Y == obj.Y && coordinate.X == obj.X)
        {
            return true;
        }
        return false;
    }

}



public class  Region
{
    public Polygon polygon;
    public Region(Double xLeft, Double xRight, Double yUp, Double yDown)
    {
        // Создание координат полигона.
        var polygonCoordinates = new Coordinate[] {
            new Coordinate(xLeft, yDown),
            new Coordinate(xLeft, yUp),
            new Coordinate(xRight, yUp),
            new Coordinate(xRight, yDown),
            new Coordinate(xLeft, yDown)
        };
        // Создание стандартного полигона по ранее созданным координатам.
        polygon = new Polygon(new LinearRing(polygonCoordinates));

    }

}

}
