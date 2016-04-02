using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.SupportPlatformDAO.Utils;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
namespace LungCare.SupportPlatform.SupportPlatformService
{
    class ExaminationPPTService
    {
        private PowerPointDAO powerPointDAO;
        private PowerPoint._Slide headSlide;
        private PowerPoint._Slide rotate3DSlide;
        private string _baseFolder;

        private int imagesNumberPerLesion = 3;
        public ExaminationPPTService()
        {
            powerPointDAO = new PowerPointDAO();
        }

        public ExaminationPPTService(string baseFolder)
        {
            powerPointDAO = new PowerPointDAO(baseFolder+"\\Lungcare.pot");
            _baseFolder = baseFolder;
        }

        public void SetHead(AirwayCT.Entity.AirwayPatient patient)
        {
            headSlide = powerPointDAO.AddNewSlide();
            powerPointDAO.AddText(patient.Name , headSlide , 50 , 50 , 300 , 40);
            powerPointDAO.AddText(patient.Age, headSlide, 50, 80, 300, 40);
            powerPointDAO.AddText(patient.Sex, headSlide, 50, 110, 300, 40);
            powerPointDAO.AddText(patient.Institution, headSlide, 50, 140, 300, 40);
            if (!string.IsNullOrEmpty(_baseFolder))
            {
                string dest3DDir = Path.Combine(_baseFolder, "Snapshot");
                string[] files = Directory.GetFiles(dest3DDir);
                if (files.Length > 0)
                {
                    powerPointDAO.AddImage(files[0], headSlide, 340, 50, 350, 350);
                }
            }
            
        }

        public void SetAll3DSlice(List<string> fileNames)
        {
            for (int i = 0; i < fileNames.Count; i++)
            {
                powerPointDAO.AddImage(fileNames[i] , 10 , 10 , 100 , 100);
            }
        }


        /// <summary>
        /// 一个病灶有三个面，每个面截三张图
        /// </summary>
        /// <param name="oneLesionAllOritationFileNames"></param>
        public void AddOneLesionSlice(List<List<string>> oneLesionAllOritationFileNames)
        {
            
           
            for (int i = 0; i < oneLesionAllOritationFileNames.Count; i++)
            {
                _Slide lesionSlide = powerPointDAO.AddNewSlide();
                for (int j = 0; j < oneLesionAllOritationFileNames[i].Count; j++)
                {
                    powerPointDAO.AddImage(oneLesionAllOritationFileNames[i][j], lesionSlide, 10, 10, 100, 100);    
                }
               // allLesionList.Add(lesionSlide);     
            }
        }

        public void AddLesionsSnapshotToPPT()
        {
            LesionEntities listLesion = LesionEntities.TestLoad(_baseFolder);
            if (listLesion != null)
            {
                for (int i = 0; i < listLesion.Count; i++)
                {
                    AddOneLesionSlice(listLesion[i]);
                }
            }
        }


        private float lesionSize = 220;
        private int lesionNumber = 0;
        public void AddOneLesionSlice(LesionEntity lesionEntity)
        {
            ++lesionNumber;
            //添加Axial方向的截图到PPT
            _Slide lesionAxialSlide = powerPointDAO.AddNewSlide();

            powerPointDAO.AddText("病灶 " + lesionNumber.ToString(), lesionAxialSlide, 30, 30, 300, 40);
            powerPointDAO.AddText("轴状位", lesionAxialSlide, 530, 30, 300, 40);
             if(!string.IsNullOrEmpty(lesionEntity.AxialCTImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.AxialCTImageFile, lesionAxialSlide, 30, 80, lesionSize, lesionSize);    
             }
              if(!string.IsNullOrEmpty(lesionEntity.AxialCTDetailImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.AxialCTDetailImageFile, lesionAxialSlide, 260, 80, lesionSize, lesionSize);    
             }     
            
              if(!string.IsNullOrEmpty(lesionEntity.Axial3DImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.Axial3DImageFile, lesionAxialSlide, 490, 80, lesionSize, lesionSize);    
             }     


            //添加Sagital方向的截图到PPT
             _Slide lesionSagitalSlide = powerPointDAO.AddNewSlide();
             powerPointDAO.AddText("矢状位", lesionSagitalSlide, 530, 30, 300, 40);
             if(!string.IsNullOrEmpty(lesionEntity.SagitalCTImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.SagitalCTImageFile, lesionSagitalSlide, 30, 80, lesionSize, lesionSize);    
             }
              if(!string.IsNullOrEmpty(lesionEntity.SagitalCTDetailImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.SagitalCTDetailImageFile, lesionSagitalSlide, 260, 80, lesionSize, lesionSize);    
             }     
            
              if(!string.IsNullOrEmpty(lesionEntity.Sagital3DImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.Sagital3DImageFile, lesionSagitalSlide, 490, 80, lesionSize, lesionSize);    
             }     



            //添加Coronal方向的截图到PPT
             _Slide lesionCoronalSlide = powerPointDAO.AddNewSlide();
             powerPointDAO.AddText("冠状位", lesionCoronalSlide, 530, 30, 300, 40);
             if(!string.IsNullOrEmpty(lesionEntity.CoronalCTImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.CoronalCTImageFile, lesionCoronalSlide, 30, 80, lesionSize, lesionSize);    
             }
              if(!string.IsNullOrEmpty(lesionEntity.CoronalCTDetailImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.CoronalCTDetailImageFile, lesionCoronalSlide, 260, 80, lesionSize, lesionSize);    
             }     
            
              if(!string.IsNullOrEmpty(lesionEntity.Coronal3DImageFile))
             {
                 powerPointDAO.AddImage(lesionEntity.Coronal3DImageFile, lesionCoronalSlide, 490, 80, lesionSize, lesionSize);    
             }     
        }



        /// <summary>
        /// 设置三维视图旋转图片的位置
        /// </summary>
        public void AddRotate3DImages()
        {
            if(!string.IsNullOrEmpty(_baseFolder))
            {
                string dest3DDir = Path.Combine(_baseFolder , "Snapshot");
                string[] files = Directory.GetFiles(dest3DDir);
                if (files.Length > 0)
                {
                    rotate3DSlide = powerPointDAO.AddNewSlide();
                    powerPointDAO.AddText("三维图像" , rotate3DSlide , 30 , 10 , 300 , 40);
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (i >= 3)
                        {
                            powerPointDAO.AddImage(files[i], rotate3DSlide, 200 * (i - 3) +(20*(i-3)) + 50, 265, 200, 200);
                        }
                        else
                        {
                            powerPointDAO.AddImage(files[i], rotate3DSlide, 200 * i +(i*20)+ 50, 50, 200, 200);
                        }
                    }
                }
            }
        }


        public void SavePPT()
        {
            powerPointDAO.Save();

        }

        public void SavePPT(string destFileName)
        {
            powerPointDAO.Save(destFileName);

        }
    }

    
}
