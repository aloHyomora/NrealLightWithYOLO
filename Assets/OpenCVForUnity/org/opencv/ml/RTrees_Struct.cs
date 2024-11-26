
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity.MlModule
{
    public partial class RTrees : DTrees
    {


        //
        // C++:  TermCriteria cv::ml::RTrees::getTermCriteria()
        //

        /// <remarks>
        ///  @see setTermCriteria
        /// </remarks>
        public Vec3d getTermCriteriaAsVec3d()
        {
            ThrowIfDisposed();

            double[] tmpArray = new double[3];
            ml_RTrees_getTermCriteria_10(nativeObj, tmpArray);
            Vec3d retVal = new Vec3d(tmpArray[0], tmpArray[1], tmpArray[2]);

            return retVal;
        }


        //
        // C++:  void cv::ml::RTrees::setTermCriteria(TermCriteria val)
        //

        /// <remarks>
        ///  @copybrief getTermCriteria @see getTermCriteria
        /// </remarks>
        public void setTermCriteria(in Vec3d val)
        {
            ThrowIfDisposed();

            ml_RTrees_setTermCriteria_10(nativeObj, (int)val.Item1, (int)val.Item2, val.Item3);


        }

    }
}