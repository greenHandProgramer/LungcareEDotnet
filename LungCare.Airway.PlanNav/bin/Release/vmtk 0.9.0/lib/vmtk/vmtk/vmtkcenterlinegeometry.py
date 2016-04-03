#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkcenterlinegeometry.py,v $
## Language:  Python
## Date:      $Date: 2006/07/17 09:52:56 $
## Version:   $Revision: 1.6 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import vtkvmtk
import sys

import pypes

vmtkcenterlinegeometry = 'vmtkCenterlineGeometry'

class vmtkCenterlineGeometry(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.Centerlines = None

        self.CurvatureArrayName = 'Curvature'
        self.TorsionArrayName = 'Torsion'
        self.FrenetTangentArrayName = 'FrenetTangent'
        self.FrenetNormalArrayName = 'FrenetNormal'
        self.FrenetBinormalArrayName = 'FrenetBinormal'
       
        self.LineSmoothing = 0
        self.OutputSmoothedLines = 0
        self.NumberOfSmoothingIterations = 100
        self.SmoothingFactor = 0.1

        self.SetScriptName('vmtkcenterlinegeometry')
        self.SetScriptDoc('compute the local geometry of centerlines in terms of curvature and torsion')
        self.SetInputMembers([
            ['Centerlines','i','vtkPolyData',1,'','the input centerlines','vmtksurfacereader'],
            ['CurvatureArrayName','curvaturearray','str',1,'','name of the array where curvature values have to be stored'],
      	    ['TorsionArrayName','torsionarray','str',1,'','name of the array where torsion values have to be stored'],
            ['FrenetTangentArrayName','frenettangentarray','str',1,'','name of the array where tangent vectors of the Frenet reference system have to be stored'],
      	    ['FrenetNormalArrayName','frenetnormalarray','str',1,'','name of the array where normal vectors of the Frenet reference system have to be stored'],
      	    ['FrenetBinormalArrayName','frenetbinormalarray','str',1,'','name of the array where binormal vectors of the Frenet reference system have to be stored'],
      	    ['LineSmoothing','smoothing','bool',1,''],
      	    ['OutputSmoothedLines','outputsmoothed','bool',1,''],
      	    ['NumberOfSmoothingIterations','iterations','int',1,'(0,)'],
      	    ['SmoothingFactor','factor','float',1,'(0.0,)']
            ])
        self.SetOutputMembers([
            ['Centerlines','o','vtkPolyData',1,'','the output centerlines','vmtksurfacewriter'],
            ['CurvatureArrayName','curvaturearray','str',1,'','name of the array where curvature values are stored'],
	          ['TorsionArrayName','torsionarray','str',1,'','name of the array where torsion values are stored']
            ])

    def Execute(self):

        if self.Centerlines == None:
            self.PrintError('Error: No input centerlines.')

        centerlineGeometry = vtkvmtk.vtkvmtkCenterlineGeometry()
        centerlineGeometry.SetInput(self.Centerlines)
      	centerlineGeometry.SetCurvatureArrayName(self.CurvatureArrayName)
      	centerlineGeometry.SetTorsionArrayName(self.TorsionArrayName)
      	centerlineGeometry.SetFrenetTangentArrayName(self.FrenetTangentArrayName)
      	centerlineGeometry.SetFrenetNormalArrayName(self.FrenetNormalArrayName)
      	centerlineGeometry.SetFrenetBinormalArrayName(self.FrenetBinormalArrayName)
        centerlineGeometry.SetLineSmoothing(self.LineSmoothing)
        centerlineGeometry.SetOutputSmoothedLines(self.OutputSmoothedLines)
        centerlineGeometry.SetNumberOfSmoothingIterations(self.NumberOfSmoothingIterations)
        centerlineGeometry.SetSmoothingFactor(self.SmoothingFactor)
        centerlineGeometry.Update()

        self.Centerlines = centerlineGeometry.GetOutput()

        if self.Centerlines.GetSource():
            self.Centerlines.GetSource().UnRegisterAllOutputs()


if __name__=='__main__':

    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
