#!/usr/bin/env python

## Program:   VMTK
## Module:    $RCSfile: vmtkicpregistration.py,v $
## Language:  Python
## Date:      $Date: 2005/09/14 09:49:59 $
## Version:   $Revision: 1.7 $

##   Copyright (c) Luca Antiga, David Steinman. All rights reserved.
##   See LICENCE file for details.

##      This software is distributed WITHOUT ANY WARRANTY; without even 
##      the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
##      PURPOSE.  See the above copyright notices for more information.


import vtk
import vtkvmtk
import sys

import pypes

vmtkicpregistration = 'vmtkICPRegistration'

class vmtkICPRegistration(pypes.pypeScript):

    def __init__(self):

        pypes.pypeScript.__init__(self)
        
        self.ReferenceSurface = None
        self.Surface = None
        self.DistanceArrayName = ''
        self.SignedDistanceArrayName = ''
        self.FarThreshold = 0.0
        self.Level = 0.0
        self.MaximumMeanDistance = 1E-2
        self.MaximumNumberOfLandmarks = 1000
        self.MaximumNumberOfIterations = 100
        self.Matrix4x4 = None

        self.FlipNormals = 0

        self.SetScriptName('vmtkicpregistration')
        self.SetScriptDoc('register a surface to a reference surface using the ICP algorithm')
        self.SetInputMembers([
            ['Surface','i','vtkPolyData',1,'','the input surface','vmtksurfacereader'],
            ['ReferenceSurface','r','vtkPolyData',1,'','the reference surface','vmtksurfacereader'],
            ['DistanceArrayName','distancearray','str',1,'','name of the array where the distance of the input surface to the reference surface has to be stored'],
            ['SignedDistanceArrayName','signeddistancearray','str',1,'','name of the array where the signed distance of the input surface to the reference surface is stored; distance is positive if distance vector and normal to the reference surface have negative dot product, i.e. if the input surface is outer with respect to the reference surface'],
            ['FarThreshold','farthreshold','float',1,'','threshold distance beyond which points are discarded during optimization'],
            ['FlipNormals','flipnormals','bool',1,'','flip normals to the reference surface after computing them'],
            ['MaximumNumberOfLandmarks','landmarks','int',1,'','maximum number of landmarks sampled from the two surfaces for evaluation of the registration metric'],
            ['MaximumNumberOfIterations','iterations','int',1,'','maximum number of iterations for the optimization problems'],
            ['MaximumMeanDistance','maxmeandistance','float',1,'','convergence threshold based on the maximum mean distance between the two surfaces']
            ])
        self.SetOutputMembers([
            ['Surface','o','vtkPolyData',1,'','the output surface','vmtksurfacewriter'],
            ['Matrix4x4','omatrix4x4','vtkMatrix4x4',1,'','the output transform matrix']
            ])

    def Execute(self):

        if self.Surface == None:
            self.PrintError('Error: No Surface.')

        if self.ReferenceSurface == None:
            self.PrintError('Error: No ReferenceSurface.')

##         if (self.SignedDistanceArrayName != '') & (self.ReferenceSurface.GetPointData().GetNormals() == None):
        if (self.SignedDistanceArrayName != ''):
            normalsFilter = vtk.vtkPolyDataNormals()
            normalsFilter.SetInput(self.ReferenceSurface)
            normalsFilter.AutoOrientNormalsOn()
            normalsFilter.ConsistencyOn()
            normalsFilter.SplittingOff()
            normalsFilter.SetFlipNormals(self.FlipNormals)
            normalsFilter.Update()
            self.ReferenceSurface.GetPointData().SetNormals(normalsFilter.GetOutput().GetPointData().GetNormals())

        self.PrintLog('Computing ICP transform.')

        icpTransform = vtkvmtk.vtkvmtkIterativeClosestPointTransform()
        icpTransform.SetSource(self.Surface)
        icpTransform.SetTarget(self.ReferenceSurface)
        icpTransform.GetLandmarkTransform().SetModeToRigidBody()
        icpTransform.StartByMatchingCentroidsOn()
        icpTransform.CheckMeanDistanceOn()
        icpTransform.SetMaximumNumberOfLandmarks(self.MaximumNumberOfLandmarks)
        icpTransform.SetMaximumNumberOfIterations(self.MaximumNumberOfIterations)
        icpTransform.SetMaximumMeanDistance(self.MaximumMeanDistance)
        if self.FarThreshold > 0.0:
            icpTransform.UseFarThresholdOn()
            icpTransform.SetFarThreshold(self.FarThreshold)
        else:
            icpTransform.UseFarThresholdOff()

        transformFilter = vtk.vtkTransformPolyDataFilter()
        transformFilter.SetInput(self.Surface)
        transformFilter.SetTransform(icpTransform)
        transformFilter.Update()

        self.PrintLog('Mean distance: '+str(icpTransform.GetMeanDistance()))

        self.Surface = transformFilter.GetOutput()
        self.Matrix4x4 = icpTransform.GetMatrix()

        if (self.DistanceArrayName != '') | (self.SignedDistanceArrayName != ''):
            self.PrintLog('Computing distance.')
            surfaceDistance = vtkvmtk.vtkvmtkSurfaceDistance()
            surfaceDistance.SetInput(self.Surface)
            surfaceDistance.SetReferenceSurface(self.ReferenceSurface)
            if (self.DistanceArrayName != ''):
                surfaceDistance.SetDistanceArrayName(self.DistanceArrayName)
            if (self.SignedDistanceArrayName != ''):
                surfaceDistance.SetSignedDistanceArrayName(self.SignedDistanceArrayName)
            surfaceDistance.Update()
            self.Surface = surfaceDistance.GetOutput()


if __name__=='__main__':
    main = pypes.pypeMain()
    main.Arguments = sys.argv
    main.Execute()
