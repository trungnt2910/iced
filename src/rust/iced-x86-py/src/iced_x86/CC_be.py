#
# SPDX-License-Identifier: MIT
# Copyright wtfsckgh@gmail.com
# Copyright iced contributors
#

# ⚠️This file was generated by GENERATOR!🦹‍♂️

# pylint: disable=invalid-name
# pylint: disable=line-too-long
# pylint: disable=too-many-lines

"""
Mnemonic condition code selector (eg. ``JBE`` / ``JNA``)
"""

from typing import List

BE: int = 0
"""
``JBE``, ``CMOVBE``, ``SETBE``
"""
NA: int = 1
"""
``JNA``, ``CMOVNA``, ``SETNA``
"""

__all__: List[str] = []