/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using System.Collections.Generic;
using System.Drawing;
using Kitware.VTK;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class TubePackage
    {
        public void SetClippingPlane(vtkPlane clippingPlane)
        {
            _mapper.RemoveAllClippingPlanes();
            _mapper.AddClippingPlane(clippingPlane);
            _mapper.Modified();
        }

        public static TubePackage Update(List<double[]> path, TubePackage package, vtkRenderer renderer)
        {
            if (package != null)
            {
                //package.RemoveMe();
                package.SetPosition(path);
                return package;
            }
            //else
            {
                TubePackage tubePackage = new TubePackage(renderer);
                tubePackage.SetPosition(path);
                return tubePackage;
            }
        }

        public void ApplyTransform(vtkMatrix4x4 matrix)
        {
            vtkMatrix4x4 t = new vtkMatrix4x4();

            vtkMatrix4x4.Multiply4x4(centerLineActor.GetMatrix(), matrix, t);

            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(t);

            this.centerLineActor.SetUserTransform(transform);
        }

        public vtkActor centerLineActor;
        private vtkRenderer renderer;
        internal vtkPolyDataMapper _mapper;

        public TubePackage(vtkRenderer aRender)
        {
            renderer = aRender;

            centerLineActor = vtkActor.New();
            aRender.AddActor(centerLineActor);
        }

        public static vtkPolyData Create(List<List<double[]>> xyzList, double radius)
        {
            vtkAppendPolyData append = new vtkAppendPolyData();
            foreach (List<double[]> list in xyzList)
            {
                append.AddInput(Create(list, radius));
            }

            append.Update();

            return append.GetOutput();
        }

        public static vtkPolyData Create(List<double[]> xyz1, double radius)
        {
            vtkPoints points = new vtkPoints();

            for (int index = 0; index < xyz1.Count; index++)
            {
                double[] doubles = xyz1[index];
                points.InsertPoint(index, doubles[0], doubles[1], doubles[2]);
            }

            vtkCellArray lines = new vtkCellArray();
            lines.InsertNextCell(xyz1.Count);

            for (int index = 0; index < xyz1.Count; index++)
            {
                lines.InsertCellPoint(index);
            }

            vtkPolyData profileData = new vtkPolyData();
            profileData.SetPoints(points);
            profileData.SetLines(lines);
            profileData.SetVerts(lines);
            profileData.Update();

            vtkCleanPolyData cleanFilter = new vtkCleanPolyData();
            cleanFilter.SetInput(profileData);
            cleanFilter.Update();

            vtkTubeFilter profileTubes = new vtkTubeFilter();
            profileTubes.SetNumberOfSides(10);
            profileTubes.SetInput(cleanFilter.GetOutput());
            //profileTubes.SetVaryRadiusToVaryRadiusByVector();
            profileTubes.SetRadius(radius);
            profileTubes.SetInputArrayToProcess(1, 0, 0, 0, "vectors");

            return profileTubes.GetOutput();
        }

        public void SetPosition(double[] xyz1, double[] xyz2, double length)
        {
            SetPosition(xyz1, VTKUtil.Extend2(xyz1, xyz2, length));
        }

        public void SetPositionFromDirection(double[] xyz1, double[] direction, double length)
        {
            SetPosition(xyz1, VTKUtil.Extend2(xyz1, VTKUtil.Add(xyz1, direction), length));
        }

        public void SetPosition(double[] xyz1, double[] xyz2)
        {
            vtkPoints points = vtkPoints.New();

            points.InsertPoint(0, xyz1[0], xyz1[1], xyz1[2]);
            points.InsertPoint(1, xyz2[0], xyz2[1], xyz2[2]);

            vtkCellArray lines = vtkCellArray.New();
            lines.InsertNextCell(2);

            lines.InsertCellPoint(0);
            lines.InsertCellPoint(1);

            vtkPolyData profileData = new vtkPolyData();
            profileData.SetPoints(points);
            profileData.SetLines(lines);
            profileData.SetVerts(lines);
            profileData.Update();

            vtkCleanPolyData cleanFilter = new vtkCleanPolyData();
            cleanFilter.SetInput(profileData);
            cleanFilter.Update();

            vtkTubeFilter profileTubes = new vtkTubeFilter();
            profileTubes.SetNumberOfSides(10);
            profileTubes.SetInput(cleanFilter.GetOutput());
            //profileTubes.SetVaryRadiusToVaryRadiusByVector();
            profileTubes.SetRadius(1);
            profileTubes.SetInputArrayToProcess(1, 0, 0, 0, "vectors");

            //vtkPolyDataMapper profileMapper = new vtkPolyDataMapper();
            //profileMapper.SetInputConnection(profileTubes.GetOutputPort());

            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInput(profileTubes.GetOutput());

            centerLineActor.SetMapper(mapper);
            centerLineActor.GetProperty().SetOpacity(0.5);
        }

        public List<double[]> XYZ
        {
            get { return _xyz; }
        }

        private List<double[]> _xyz;
        private double radius = 0.5;

        public void SetPosition(List<double[]> xyz1, List<double> radius)
        {
            this._xyz = xyz1;
            _points = vtkPoints.New();

            for (int index = 0; index < xyz1.Count; index++)
            {
                double[] doubles = xyz1[index];
                _points.InsertPoint(index, doubles[0], doubles[1], doubles[2]);
            }

            _lines = vtkCellArray.New();
            _lines.InsertNextCell(xyz1.Count);

            for (int index = 0; index < xyz1.Count; index++)
            {
                _lines.InsertCellPoint(index);
            }

            _profileData = vtkPolyData.New();
            _profileData.SetPoints(_points);
            _profileData.SetLines(_lines);
            _profileData.BuildLinks((int)_points.GetNumberOfPoints());

            vtkUnsignedCharArray radiusArray = vtkUnsignedCharArray.New();
            radiusArray.SetNumberOfComponents(1);
            foreach (var item in radius)
            {
                radiusArray.InsertNextTuple1(Math.Max(1, item));
            }

            _profileData.GetPointData().SetScalars(radiusArray);

            _profileData.Update();

            _cleanFilter = vtkCleanPolyData.New();
            _cleanFilter.SetInput(_profileData);
            _cleanFilter.Update();

            _profileTubes = vtkTubeFilter.New();
            //_profileTubes.CappingOn();
            _profileTubes.SetNumberOfSides(20);
            //_profileTubes.SetInput(_cleanFilter.GetOutput());
            _profileTubes.SetInput(_profileData);
            _profileTubes.SetVaryRadiusToVaryRadiusOff();
            _profileTubes.SetVaryRadiusToVaryRadiusByScalar();

            double[] tuberad_range = radiusArray.GetRange();

            _profileTubes.SetRadius(tuberad_range[0]);
            _profileTubes.SetRadiusFactor(tuberad_range[1] / tuberad_range[0]);

            //_profileTubes.SetRadius(0.5);
            //_profileTubes.SetRadiusFactor(20);
            //_profileTubes.Update();

            //_profileTubes.SetInputArrayToProcess(1, 0, 0, 0, "vectors");

            //vtkPolyDataMapper profileMapper = new vtkPolyDataMapper();
            //profileMapper.SetInputConnection(profileTubes.GetOutputPort());

            PolyData = _profileTubes.GetOutput();

            _mapper = vtkPolyDataMapper.New();
            _mapper.ScalarVisibilityOff();
            _mapper.SetInput(_profileTubes.GetOutput());

            centerLineActor.SetMapper(_mapper);
        }

        public void SetPosition(List<double[]> xyz1)
        {
            if (_points == null)
            {
                this._xyz = xyz1;
                _points = vtkPoints.New();

                for (int index = 0; index < xyz1.Count; index++)
                {
                    double[] doubles = xyz1[index];
                    _points.InsertPoint(index, doubles[0], doubles[1], doubles[2]);
                }

                _lines = vtkCellArray.New();
                _lines.InsertNextCell(xyz1.Count);

                for (int index = 0; index < xyz1.Count; index++)
                {
                    _lines.InsertCellPoint(index);
                }

                _profileData = vtkPolyData.New();
                _profileData.SetPoints(_points);
                _profileData.SetLines(_lines);
                _profileData.SetVerts(_lines);
                _profileData.Update();

                _cleanFilter = vtkCleanPolyData.New();
                _cleanFilter.SetInput(_profileData);
                _cleanFilter.Update();

                _profileTubes = vtkTubeFilter.New();
                _profileTubes.CappingOn();
                _profileTubes.SetNumberOfSides(10);
                _profileTubes.SetInput(_cleanFilter.GetOutput());
                //profileTubes.SetVaryRadiusToVaryRadiusByVector();

                _profileTubes.SetRadius(radius);
                _profileTubes.SetInputArrayToProcess(1, 0, 0, 0, "vectors");

                //vtkPolyDataMapper profileMapper = new vtkPolyDataMapper();
                //profileMapper.SetInputConnection(profileTubes.GetOutputPort());

                PolyData = _profileTubes.GetOutput();

                _mapper = vtkPolyDataMapper.New();
                _mapper.SetInput(_profileTubes.GetOutput());

                centerLineActor.SetMapper(_mapper);
                centerLineActor.GetProperty().SetOpacity(0.5);
            }
            else
            {
                vtkPoints _newpoints = vtkPoints.New();

                for (int index = 0; index < xyz1.Count; index++)
                {
                    double[] doubles = xyz1[index];
                    _newpoints.InsertPoint(index, doubles[0], doubles[1], doubles[2]);
                }

                vtkCellArray _newlines = vtkCellArray.New();
                _newlines.InsertNextCell(xyz1.Count);

                for (int index = 0; index < xyz1.Count; index++)
                {
                    _newlines.InsertCellPoint(index);
                }

                _profileData.SetPoints(_newpoints);
                _profileData.SetLines(_newlines);
                _profileData.SetVerts(_newlines);
                _profileData.Update();

                _profileTubes.SetRadius(radius);

                _points.Dispose();
                _lines.Dispose();

                _points = _newpoints;
                _lines = _newlines;

                _points.Modified();
                _mapper.Modified();
            }
        }

        public vtkPolyData PolyData;
        public void SetRadius(double radius)
        {
            this.radius = radius;
        }

        public void SetRadiusAndUpdate(double radius)
        {
            this.radius = radius;
            SetPosition(_xyz);
        }

        public void RandColor()
        {
            SetColor(vtkMath.Random(0.5, 1), vtkMath.Random(0.5, 1), vtkMath.Random(0.5, 1));
        }

        public void SetColor(double r, double g, double b)
        {
            centerLineActor.GetProperty().SetColor(r, g, b);
        }

        public void SetSize(double size)
        {
            centerLineActor.SetScale(size);
        }

        public void SetOpacity(double alpha)
        {
            centerLineActor.GetProperty().SetOpacity(alpha);
        }

        private vtkPoints _points;
        private vtkCellArray _lines;
        private vtkPolyData _profileData;
        private vtkCleanPolyData _cleanFilter;
        private vtkTubeFilter _profileTubes;

        public void RemoveMe()
        {
            if (centerLineActor != null)
            {
                renderer.RemoveActor(centerLineActor);
                centerLineActor.Dispose();
                centerLineActor = null;
            }

            _cleanFilter.Dispose();
            _profileTubes.Dispose();
            _profileData.Dispose();
            _lines.Dispose();
            _points.Dispose();
            this._mapper.Dispose();
        }

        public Color GetColor()
        {
            return Color.FromArgb(
                (int)(centerLineActor.GetProperty().GetColor()[0] * 255),
                (int)(centerLineActor.GetProperty().GetColor()[1] * 255),
                (int)(centerLineActor.GetProperty().GetColor()[2] * 255));
        }
    }
}
